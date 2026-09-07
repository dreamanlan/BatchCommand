@echo on
rem working directory

rem 1、访问 https://ilinkai.weixin.qq.com/ilink/bot/get_bot_qrcode?bot_type=3，得到一个扫码链接与二维码code
rem 2、微信扫码
rem 3、微信扫码后，访问 https://ilinkai.weixin.qq.com/ilink/bot/get_qrcode_status?qrcode=<qrcode> 来获取二维码状态，arcode传1中code
rem 4、将二维码状态文件内容覆盖到wechat-credentials.json，然后重启wechat-bridge-for-agent.js

set driver=%~d0
set workdir=%~dp0
cd /d "%workdir%"

d:/nodejs/node wechat-bridge-for-agent.js
