export interface NoticeItem {
  id: number
  title: string
  content: string
  noticeType: number
  status: number
  publishTime?: string
  createBy?: number
  createByName?: string
  createTime?: string
  updateBy?: number
  updateByName?: string
  updateTime?: string
}

export interface NoticePushItem {
  id: number
  title: string
  content: string
  noticeType: number
  publishTime?: string
}
