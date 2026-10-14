# TestService · ERP 后端服务

基于 **ASP.NET Core 8** 的 ERP 后端服务，对接 **Nginx / Redis / RabbitMQ / MySQL**，
采用 **洋葱架构**（Domain → Application → Infrastructure → Web），ORM 使用 **EF Core**。

---

## 1. 项目结构

```
TestService/
├── TestService.sln                          # 解决方案
├── TestService.Domain/                      # 领域层：实体、领域接口、领域异常
│   ├── Common/                              # Entity / FullAuditedEntity / Result
│   ├── Entities/                            # SysUser、SysRole、SysUserRole、CusCustomer、ProProduct、SoSalesOrder、SoSalesOrderItem
│   ├── Exceptions/                          # DomainException
│   └── Interfaces/                          # IRepository、ICacheService、IMessageBus、IUnitOfWork
├── TestService.Application/                 # 应用层：服务接口/实现、DTOs
│   ├── Common/                              # ApiResponse、PagedRequest、PredicateExtensions
│   ├── Dtos/                                # AuthDtos / CustomerDtos / ProductDtos / SalesOrderDtos
│   ├── Interfaces/                          # IAuthService / ICustomerService / IProductService / ISalesOrderService / IJwtTokenService / IPasswordHasher / ICurrentUser
│   ├── Services/                            # 服务实现
│   └── ServiceCollectionExtensions.cs
├── TestService.Infrastructure/              # 基础设施层：EF Core、Redis、RabbitMQ、JWT
│   ├── Cache/                               # RedisCacheService（StackExchange.Redis）
│   ├── Identity/                            # JwtTokenService、BCryptPasswordHasher、CurrentUser
│   ├── Messaging/                           # RabbitMqMessageBus（IHostedService）
│   ├── Options/                             # JwtOptions / RedisOptions / RabbitMqOptions
│   ├── Persistence/                         # AppDbContext、Repository、DbInitializer
│   └── ServiceCollectionExtensions.cs
├── TestService/                             # Web 层
│   ├── Controllers/                         # AuthController / CustomerController / ProductController / SalesOrderController / HealthController
│   ├── Filters/                             # GlobalExceptionFilter
│   ├── Middlewares/                         # AuthMiddleware（JWT 鉴权 + 白名单）
│   ├── Program.cs
│   ├── appsettings.json
│   └── TestService.csproj
├── deploy/
│   └── nginx.conf                           # Nginx 反向代理配置示例
├── Dockerfile                               # 多阶段构建镜像
├── .dockerignore
├── .gitignore
└── README.md
```

依赖方向（洋葱）：

```
Web ─────► Application ─────► Domain ◄───── Infrastructure
                                    └───── Application（实现接口）
```

---

## 2. 外部服务依赖

| 服务       | 默认端口 | 用途                              | 配置项                                    |
| ---------- | -------- | --------------------------------- | ----------------------------------------- |
| MySQL      | 3306     | 主数据存储（EF Core + Pomelo）    | `ConnectionStrings:MySql`                 |
| Redis      | 6379     | 缓存 / 会话 / 分布式锁            | `Redis:ConnectionString`                  |
| RabbitMQ   | 5672     | 事件总线（订单创建/确认等事件）   | `RabbitMQ:HostName/Port/...`              |
| Nginx      | 80/443   | 反向代理 + 负载均衡 + HTTPS 终结  | `deploy/nginx.conf`                       |

> 外部服务均非强依赖：Redis / RabbitMQ 连接失败时业务降级为本地日志，MySQL 失败时启动会失败。

---

## 3. ERP 核心表

| 表名                  | 实体             | 用途                         |
| --------------------- | ---------------- | ---------------------------- |
| `sys_user`            | SysUser          | 用户                         |
| `sys_role`            | SysRole          | 角色                         |
| `sys_user_role`       | SysUserRole      | 用户-角色关联                |
| `cus_customer`        | CusCustomer      | 客户档案                     |
| `pro_product`         | ProProduct       | 商品档案                     |
| `so_sales_order`      | SoSalesOrder     | 销售订单头                   |
| `so_sales_order_item` | SoSalesOrderItem | 销售订单明细                 |

所有表均启用软删除（`is_deleted`）与全局查询过滤。

---

## 4. API 接口清单

### 4.1 鉴权

| 方法   | 路径                  | 鉴权 | 说明                |
| ------ | --------------------- | ---- | ------------------- |
| POST   | `/api/auth/login`     | 白名单 | 用户登录，返回 JWT  |
| POST   | `/api/auth/register`  | 白名单 | 用户注册            |
| GET    | `/api/auth/me`        | 需登录 | 当前登录用户信息   |
| GET    | `/api/health`         | 白名单 | 健康检查            |

### 4.2 客户管理

| 方法   | 路径                       | 鉴权 | 说明               |
| ------ | -------------------------- | ---- | ------------------ |
| GET    | `/api/customer?pageIndex&pageSize&keyword&level&status` | 需登录 | 分页查询 |
| GET    | `/api/customer/{id}`       | 需登录 | 客户详情         |
| POST   | `/api/customer`            | 需登录 | 新增客户         |
| PUT    | `/api/customer/{id}`       | 需登录 | 修改客户         |
| DELETE | `/api/customer/{id}`       | 需登录 | 删除（软删）     |

### 4.3 商品管理

| 方法   | 路径                                              | 鉴权 | 说明       |
| ------ | ------------------------------------------------- | ---- | ---------- |
| GET    | `/api/product?pageIndex&pageSize&keyword&category&status` | 需登录 | 分页 |
| GET    | `/api/product/{id}`                               | 需登录 | 详情   |
| POST   | `/api/product`                                    | 需登录 | 新增   |
| PUT    | `/api/product/{id}`                               | 需登录 | 修改   |
| DELETE | `/api/product/{id}`                               | 需登录 | 删除   |

### 4.4 销售订单

| 方法   | 路径                                          | 鉴权 | 说明                           |
| ------ | --------------------------------------------- | ---- | ------------------------------ |
| GET    | `/api/salesorder?pageIndex&pageSize&keyword&status&customerId` | 需登录 | 分页 |
| GET    | `/api/salesorder/{id}`                        | 需登录 | 订单详情（含明细）       |
| POST   | `/api/salesorder`                             | 需登录 | 新建订单（草稿）         |
| POST   | `/api/salesorder/{id}/confirm`                | 需登录 | 确认订单（扣减库存）     |
| POST   | `/api/salesorder/{id}/cancel`                 | 需登录 | 取消订单                 |
| DELETE | `/api/salesorder/{id}`                        | 需登录 | 删除订单                 |

---

## 5. 统一响应

所有接口返回 `ApiResponse<T>`：

```json
{
  "code": 0,
  "message": "操作成功",
  "data": { ... },
  "timestamp": 1737012345678
}
```

- `code = 0`：成功
- `code = 1`：业务失败（参数/业务规则）
- `code = 401`：未授权
- `code = 404`：未找到
- `code = 500`：服务器内部错误

---

## 6. 快速开始

### 6.1 启动依赖（推荐 Docker）

```bash
docker run -d --name mysql      -p 3306:3306 -e MYSQL_ROOT_PASSWORD=root mysql:8
docker run -d --name redis      -p 6379:6379 redis:7
docker run -d --name rabbitmq   -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 6.2 修改配置

编辑 `TestService/appsettings.json` 中的 `ConnectionStrings:MySql` / `Redis:ConnectionString` / `RabbitMQ:HostName`。

### 6.3 启动服务

```bash
dotnet restore
dotnet run --project TestService
```

首次启动会自动：
- 调用 `EnsureCreated` 建表
- 注入默认账号 `admin / admin123`

### 6.4 调用示例

```bash
# 登录
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
# 返回：{ "code":0, "data":{ "accessToken":"eyJhbGc...", "user":{...} } }

# 调用受保护接口
curl http://localhost:5000/api/customer \
  -H "Authorization: Bearer eyJhbGc..."

# 创建客户
curl -X POST http://localhost:5000/api/customer \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"code":"C001","name":"北京华联","contactName":"张三","contactPhone":"13800138000","level":2}'

# 创建销售订单
curl -X POST http://localhost:5000/api/salesorder \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "customerId":"<customer-guid>",
    "discountAmount": 10,
    "remark":"测试订单",
    "items":[{"productId":"<product-guid>","quantity":2}]
  }'

# 确认订单（扣减库存 + 发布 RabbitMQ 事件）
curl -X POST http://localhost:5000/api/salesorder/<order-id>/confirm \
  -H "Authorization: Bearer <token>"
```

---

## 7. 生产部署

### 7.1 Docker 镜像

```bash
docker build -t testservice:latest .
docker run -d --name testservice -p 5000:5000 \
  -e ConnectionStrings__MySql="server=mysql;port=3306;database=testservice;user=root;password=xxx" \
  -e Redis__ConnectionString="redis:6379" \
  -e RabbitMQ__HostName="rabbitmq" \
  testservice:latest
```

### 7.2 Nginx 反向代理

参见 `deploy/nginx.conf`。核心要点：

- 转发 `X-Forwarded-For` / `X-Forwarded-Proto`（后端已通过 `UseForwardedHeaders` 解析真实客户端 IP）
- `client_max_body_size 20m`（按业务调整）
- 启用 gzip 压缩 JSON
- `location = /api/health` 用于负载均衡健康检查

---

## 8. 后续可扩展

- EF Core Migrations 替代 `EnsureCreated`
- 集成 SkyWalking / Prometheus 指标
- 多租户（`SysTenant` 实体 + 全局查询过滤器）
- 分布式锁（Redis LockService）防止库存超卖
- OpenTelemetry 链路追踪（跨 ASP.NET Core → RabbitMQ → 下游消费者）
