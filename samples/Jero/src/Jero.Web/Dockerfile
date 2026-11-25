FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS base
WORKDIR /app
EXPOSE 5095

# 构建上下文为 dotnet publish 输出目录，包含后端程序集及 Jolt 静态资源。
COPY . .

# 设置语言/区域设置环境变量
ENV LANG zh-Hans

# 更新包管理器并安装free命令
RUN apt-get update && apt-get install -y procps

ENTRYPOINT ["dotnet", "Jero.Web.dll"]
