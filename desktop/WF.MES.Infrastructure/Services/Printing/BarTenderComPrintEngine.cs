using System.IO;
using System.Runtime.InteropServices;
using Serilog;
using WF.MES.Core.Exceptions;

namespace WF.MES.Infrastructure.Services.Printing;

/// <summary>封装 BarTender.Application COM：开模板、写 CSV 数据库、PrintOut。</summary>
internal sealed class BarTenderComPrintEngine : IDisposable
{
    private const int BtDoNotSaveChanges = 0;
    private const string ApplicationProgId = "BarTender.Application";

    private readonly dynamic _application;
    private dynamic? _format;
    private bool _disposed;

    private BarTenderComPrintEngine(dynamic application)
    {
        _application = application;
    }

    public static BarTenderComPrintEngine Create()
    {
        var type = Type.GetTypeFromProgID(ApplicationProgId)
            ?? throw new BusinessException("err.bartenderComNotFound", ApplicationProgId);

        dynamic application = Activator.CreateInstance(type) ?? throw new BusinessException("err.bartenderStartFailed");

        application.Visible = false;
        Log.Information("BarTender COM 打印引擎已启动");
        return new BarTenderComPrintEngine(application);
    }

    public void PrintJobs(
        string templatePath,
        string printerName,
        IReadOnlyList<IReadOnlyDictionary<string, string>> jobs,
        IProgress<int>? progress)
    {
        OpenFormat(templatePath);
        try
        {
            dynamic? printSetup = null;
            try
            {
                printSetup = _format!.PrintSetup;
                printSetup.Printer = printerName;

                var layout = ReadPageLayout();
                var databaseFieldName = ReadPrimaryDatabaseFieldName();
                Log.Information(
                    "BarTender 模板布局 {Rows} 行 × {Columns} 列，每页 {LabelsPerPage} 张，数据库字段 {FieldName}，本次共 {Total} 条",
                    layout.Rows,
                    layout.Columns,
                    layout.LabelsPerPage,
                    databaseFieldName,
                    jobs.Count);

                var csvPath = BarTenderPrintDatabaseWriter.WriteBarcodeCsv(jobs, databaseFieldName);
                try
                {
                    BindTextDatabase(csvPath);
                    printSetup.IdenticalCopiesOfLabel = 1;
                    _format.PrintOut(false, false);
                    progress?.Report(jobs.Count);
                }
                finally
                {
                    TryDeleteFile(csvPath);
                }
            }
            finally
            {
                ComObject.Release(printSetup);
            }
        }
        finally
        {
            CloseFormat();
        }
    }

    private BarTenderPageLayout ReadPageLayout()
    {
        EnsureFormatOpen();

        dynamic? pageSetup = null;
        try
        {
            pageSetup = _format!.PageSetup;
            var rows = ReadIntProperty(pageSetup, 1, "LabelRows", "NumberOfRows", "Rows");
            var columns = ReadIntProperty(pageSetup, 1, "LabelColumns", "NumberOfColumns", "Columns");
            return new BarTenderPageLayout(rows, columns);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "无法读取 BarTender 页面布局，使用默认 1×1");
            return new BarTenderPageLayout(1, 1);
        }
        finally
        {
            ComObject.Release(pageSetup);
        }
    }

    private static int ReadIntProperty(dynamic target, int defaultValue, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            try
            {
                var value = ((object)target).GetType().InvokeMember(
                    propertyName,
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    target,
                    null);

                if (value != null)
                {
                    return Math.Max(1, Convert.ToInt32(value));
                }
            }
            catch
            {
            }
        }

        return defaultValue;
    }

    private string ReadPrimaryDatabaseFieldName()
    {
        EnsureFormatOpen();

        dynamic? databases = null;
        dynamic? database = null;
        dynamic? fields = null;
        try
        {
            databases = _format!.Databases;
            if (Convert.ToInt32(databases.Count) < 1)
            {
                throw new BusinessException("err.bartenderNoDatabase");
            }

            database = databases.GetDatabase(1);
            fields = database.Fields;
            if (Convert.ToInt32(fields.Count) < 1)
            {
                throw new BusinessException("err.bartenderNoDatabaseFields");
            }

            return ReadFieldName(fields, 1);
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "读取 BarTender 数据库字段名失败，回退为 {Fallback}", BarcodeLabelPrintRequests.BarcodeFieldName);
            return BarcodeLabelPrintRequests.BarcodeFieldName;
        }
        finally
        {
            ComObject.Release(fields);
            ComObject.Release(database);
            ComObject.Release(databases);
        }
    }

    private static string ReadFieldName(dynamic fields, int index)
    {
        dynamic? field = null;
        try
        {
            try
            {
                field = fields.GetField(index);
                var name = Convert.ToString(field.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }
            catch
            {
                ComObject.Release(field);
                field = fields[index];
                var name = Convert.ToString(field.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }

            throw new BusinessException("err.bartenderFieldNameUnreadable");
        }
        finally
        {
            ComObject.Release(field);
        }
    }

    private void BindTextDatabase(string csvPath)
    {
        EnsureFormatOpen();
        _format!.UseDatabase = true;

        dynamic? databases = null;
        dynamic? database = null;
        try
        {
            databases = _format.Databases;
            if (Convert.ToInt32(databases.Count) < 1)
            {
                throw new BusinessException("err.bartenderDatabaseSetup");
            }

            database = databases.GetDatabase(1);
            database.TextFile.FileName = csvPath;
            TryRefreshDatabase(database);
        }
        finally
        {
            ComObject.Release(database);
            ComObject.Release(databases);
        }
    }

    private static void TryRefreshDatabase(dynamic database)
    {
        dynamic? textFile = null;
        try
        {
            textFile = database.TextFile;
            textFile.Refresh();
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "BarTender 数据库 Refresh 不可用，已忽略");
        }
        finally
        {
            ComObject.Release(textFile);
        }
    }

    private void OpenFormat(string templatePath)
    {
        _format = _application.Formats.Open(templatePath, false, string.Empty);
    }

    private void EnsureFormatOpen()
    {
        if (_format == null)
        {
            throw new BusinessException("err.bartenderFormatNotOpen");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        CloseFormat();

        try
        {
            _application.Quit(BtDoNotSaveChanges);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "退出 BarTender COM 应用时发生异常");
        }
        finally
        {
            ComObject.Release(_application);
        }
    }

    private void CloseFormat()
    {
        if (_format == null)
        {
            return;
        }

        try
        {
            _format.Close(BtDoNotSaveChanges);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "关闭 BarTender 模板时发生异常");
        }
        finally
        {
            ComObject.Release(_format);
            _format = null;
        }
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "删除临时打印数据文件失败：{Path}", path);
        }
    }

    private static class ComObject
    {
        public static void Release(object? comObject)
        {
            if (comObject == null)
            {
                return;
            }

            try
            {
                while (Marshal.ReleaseComObject(comObject) > 0)
                {
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "释放 COM 对象时发生异常");
            }
        }
    }

}
