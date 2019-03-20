namespace AliyunDdnsCSharp.Model
{
    public class SetDomainRecordStatusResponse:BaseResponse
    {
        /// <summary>
        /// 解析记录的ID
        /// </summary>
        public string RecordId { get; set; }
        /// <summary>
        /// 当前解析记录状态
        /// </summary>
        public string Status { get; set; }
    }
}
