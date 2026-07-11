import type { DeptItem } from '@/types/system/dept'

/** 行政组织架构仅展示：集团(1) / 公司(2) / 部门(3) */
export function filterAdminOrgTree(nodes: DeptItem[], maxType = 3): DeptItem[] {
  return nodes
    .filter((node) => node.deptType <= maxType)
    .map((node) => ({
      ...node,
      children: node.children?.length ? filterAdminOrgTree(node.children, maxType) : []
    }))
}
