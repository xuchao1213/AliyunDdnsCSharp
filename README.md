# AliyunDdnsCSharp

### 介绍

基于阿里云最新云解析API编写的DDNS Windows Services 程序，可将本机公网IP实时更新到自己阿里云的域名解析记录中

### 特点

1. 支持IPV4
2. 支持IPV6

### 依赖

1. .Net Framework >=4.0 (win7 即以下安装失败时请尝试安装.net 4.0 KB2468871这个补丁 下载链接 https://www.microsoft.com/zh-CN/download/details.aspx?id=3556)  

### 使用说明

1. 在[阿里云](https://www.aliyun.com/)申请一个域名
2. 阿里云域名控制台[申请AccessId Key和Secrect](https://ak-console.aliyun.com/#/accesskey)
3. 安装：（下面两种方式任选一）
   - git clone 本项目代码到本机用vs（2013及以上版本）编译,将生成的`AliyunDdnsCSharp.exe`程序及相关依赖文件放在任意目录,在CMD中执行AliyunDdnsCSharp.exe -i(AliyunDdnsCSharp.exe /i)即可安装为服务，
   - 直接到[Release](../../releases)下载压缩包`AliyunDdnsCSharp.zip`直接使用解压到任意目录，双击Install.bat即可
4. 在程序所在目录下conf下放置配置文件（参照example.foo.com.conf配置）
5. 重启电脑（或手动启动AliyunDdns服务）

### 配置说明

1. 配置示例 ：example.foo.com.conf

 ```json
{
      "Interval": "刷新间隔，单位分钟",
      "AccessKeyId": "阿里云AccessKeyId See https://help.aliyun.com/knowledge_detail/38738.html?spm=5176.11065259.1996646101.searchclickresult.73c9490e2I0S3U",
      "AccessKeySecret": "阿里云AccessKeySecret",
      "DomainName": "阿里云域名 如 google.com",
      "SubDomainName": "阿里云子域名 如 test",
      "Type": "A/AAAA,目前仅支持 A(IPV4)、AAAA(IpV6),默认:A",
	  "Line":"解析线路，默认为default。参见解析线路枚举 https://help.aliyun.com/document_detail/29807.html?spm=a2c4g.11186623.2.22.41dd2846rHiL1v",
      "TTL":"600,生存时间，默认为600秒（10分钟），参见TTL定义说明 https://help.aliyun.com/document_detail/29806.html?spm=a2c4g.11186623.2.18.7cde1cebY1cQtc",
      "GetIpUrls": [
        "获取外网Ip的地址",
        "支持多个配置",
        "IPV4不填写默认从 http://ip.hiyun.me获取IPV6地址",
        "IPV6不填写默认从IFCONFIG获取IPV6地址"
       ]
}
 ```
2. 支持多个配置文件，每个配置文件单独配置一条记录  

### 附

支持获取IPV4地址的网址列表：

1.  http://ip.hiyun.me (自己搭建的 )
2.  https://ip.cn
3.  ~~http://www.ip138.com~~
4.  http://ip.zxinc.org/getip
5.  http://v4.ipv6-test.com/api/myip.php

支持获取IPV6地址的网址列表：
（感谢 [wowplayer](https://gitee.com/wowplayer) 提供）
1. http://v4v6.ipv6-test.com/api/myip.php (V4 & V6)
2. http://v6.ip.zxinc.org/getip
2. http://v6.ipv6-test.com/api/myip.php

