CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `cus_customer` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Code` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `ContactName` varchar(64) CHARACTER SET utf8mb4 NULL,
    `ContactPhone` varchar(32) CHARACTER SET utf8mb4 NULL,
    `ContactEmail` varchar(128) CHARACTER SET utf8mb4 NULL,
    `Address` varchar(255) CHARACTER SET utf8mb4 NULL,
    `Level` int NOT NULL,
    `Status` int NOT NULL,
    `Remark` varchar(500) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `UpdatedAt` datetime(6) NULL,
    `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    CONSTRAINT `PK_cus_customer` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `pro_product` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Code` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `Category` varchar(64) CHARACTER SET utf8mb4 NULL,
    `Unit` varchar(16) CHARACTER SET utf8mb4 NOT NULL,
    `Price` decimal(18,2) NOT NULL,
    `CostPrice` decimal(18,2) NOT NULL,
    `StockQty` int NOT NULL,
    `Status` int NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `UpdatedAt` datetime(6) NULL,
    `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    CONSTRAINT `PK_pro_product` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `sys_role` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Name` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Code` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `Status` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `UpdatedAt` datetime(6) NULL,
    `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    CONSTRAINT `PK_sys_role` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `sys_user` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Username` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `PasswordHash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Salt` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `RealName` varchar(64) CHARACTER SET utf8mb4 NULL,
    `Email` varchar(128) CHARACTER SET utf8mb4 NULL,
    `Phone` varchar(32) CHARACTER SET utf8mb4 NULL,
    `Status` int NOT NULL,
    `LastLoginTime` datetime(6) NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `UpdatedAt` datetime(6) NULL,
    `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    CONSTRAINT `PK_sys_user` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `so_sales_order` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `OrderNo` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `CustomerId` char(36) COLLATE ascii_general_ci NOT NULL,
    `OrderDate` datetime(6) NOT NULL,
    `TotalAmount` decimal(18,2) NOT NULL,
    `DiscountAmount` decimal(18,2) NOT NULL,
    `FinalAmount` decimal(18,2) NOT NULL,
    `Status` int NOT NULL,
    `Remark` varchar(500) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `UpdatedAt` datetime(6) NULL,
    `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    CONSTRAINT `PK_so_sales_order` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_so_sales_order_cus_customer_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `cus_customer` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `sys_user_role` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `RoleId` char(36) COLLATE ascii_general_ci NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `UpdatedAt` datetime(6) NULL,
    `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_sys_user_role` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_sys_user_role_sys_role_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `sys_role` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_sys_user_role_sys_user_UserId` FOREIGN KEY (`UserId`) REFERENCES `sys_user` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `so_sales_order_item` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `OrderId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ProductId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Quantity` int NOT NULL,
    `UnitPrice` decimal(18,2) NOT NULL,
    `SubTotal` decimal(18,2) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `UpdatedAt` datetime(6) NULL,
    `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    CONSTRAINT `PK_so_sales_order_item` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_so_sales_order_item_pro_product_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `pro_product` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_so_sales_order_item_so_sales_order_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `so_sales_order` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_cus_customer_Code` ON `cus_customer` (`Code`);

CREATE UNIQUE INDEX `IX_pro_product_Code` ON `pro_product` (`Code`);

CREATE INDEX `IX_so_sales_order_CustomerId` ON `so_sales_order` (`CustomerId`);

CREATE UNIQUE INDEX `IX_so_sales_order_OrderNo` ON `so_sales_order` (`OrderNo`);

CREATE INDEX `IX_so_sales_order_item_OrderId` ON `so_sales_order_item` (`OrderId`);

CREATE INDEX `IX_so_sales_order_item_ProductId` ON `so_sales_order_item` (`ProductId`);

CREATE UNIQUE INDEX `IX_sys_role_Code` ON `sys_role` (`Code`);

CREATE UNIQUE INDEX `IX_sys_user_Username` ON `sys_user` (`Username`);

CREATE INDEX `IX_sys_user_role_RoleId` ON `sys_user_role` (`RoleId`);

CREATE UNIQUE INDEX `IX_sys_user_role_UserId_RoleId` ON `sys_user_role` (`UserId`, `RoleId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261019063417_InitialSchema', '8.0.11');

COMMIT;

