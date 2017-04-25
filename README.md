# AliyunDdnsCSharp
基于阿里云最新云解析API编写的DDNS Windows Services 程序，可将本机公网IP实时更新到自己阿里云的域名解析记录中

## 使用方法
1. 在阿里云申请一个域名
2. 到阿里云域名控制台[申请AccessId Key和Secrect](https://ak-console.aliyun.com/#/accesskey)
3. Clone本项目代码到本机用vs（2013及以上版本）编译，将生成的`AliyunDdnsCSharp.exe`程序放在任意目录
4. 打开cmd 并cd至上一步程序所在目录
5. 执行AliyunDdnsCSharp.exe -i(AliyunDdnsCSharp.exe /i)即可安装为服务，
6. 在程序所在目录下新建config.txt，并按要求配置
6. 重启电脑（或手动启动AliyunDdns服务）即可
