# 多阶段构建：第一阶段编译，第二阶段运行
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制解决方案与项目文件
COPY TestService.sln ./
COPY TestService.Domain/TestService.Domain.csproj TestService.Domain/
COPY TestService.Application/TestService.Application.csproj TestService.Application/
COPY TestService.Infrastructure/TestService.Infrastructure.csproj TestService.Infrastructure/
COPY TestService/TestService.csproj TestService/

# 还原
RUN dotnet restore TestService.sln

# 复制源码并发布
COPY . .
RUN dotnet publish TestService/TestService.csproj -c Release -o /app/publish --no-restore

# 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
EXPOSE 5000

# 入口：dotnet 启动应用
ENTRYPOINT ["dotnet", "TestService.dll"]
