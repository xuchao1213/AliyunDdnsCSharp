/*--------------------------------------------------------
* 
* File: FormatType
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-10-10 22:18:23
* Desc: 返回值的类型 
* 
* -------------------------------------------------------*/

namespace AliyunDdnsCSharp.Model
{
    /// <summary>
    /// 返回值的类型，支持JSON与XML。默认为XML
    /// </summary>
    public enum FormatType
    {
        XML,
        JSON,
        DEFAULT = JSON,
    }
}
