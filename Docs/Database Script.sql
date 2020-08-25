IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

CREATE TABLE [Categories] (
    [Id] bigint NOT NULL IDENTITY,
    [Title] nvarchar(255) NULL,
    [Image] varbinary(max) NULL,
    [ParentId] bigint NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Categories_Categories_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Provinces] (
    [Id] bigint NOT NULL IDENTITY,
    [Name] nvarchar(255) NOT NULL,
    CONSTRAINT [PK_Provinces] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Users] (
    [Id] bigint NOT NULL IDENTITY,
    [Username] nvarchar(255) NULL,
    [Password] nvarchar(max) NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [NationalCode] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [EmailAddress] nvarchar(255) NULL,
    [MobileNumber] nvarchar(255) NULL,
    [Type] int NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Cities] (
    [Id] bigint NOT NULL IDENTITY,
    [ProvinceId] bigint NOT NULL,
    [Name] nvarchar(255) NOT NULL,
    [IsCenter] bit NOT NULL,
    CONSTRAINT [PK_Cities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Cities_Provinces_ProvinceId] FOREIGN KEY ([ProvinceId]) REFERENCES [Provinces] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [RefreshTokens] (
    [Id] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [Token] nvarchar(max) NULL,
    [Expires] datetime2 NOT NULL,
    [Created] datetime2 NOT NULL,
    [CreatedByIp] nvarchar(255) NULL,
    [Revoked] datetime2 NULL,
    [RevokedByIp] nvarchar(255) NULL,
    [ReplacedByToken] nvarchar(max) NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [UserActivationCodes] (
    [Id] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [Code] int NOT NULL,
    [IssueTime] datetime2 NOT NULL,
    [ExpireTime] datetime2 NOT NULL,
    CONSTRAINT [PK_UserActivationCodes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserActivationCodes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Addresses] (
    [Id] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [CityId] bigint NOT NULL,
    [PostalCode] nvarchar(10) NULL,
    [Details] nvarchar(max) NULL,
    [Location] geography NULL,
    CONSTRAINT [PK_Addresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Addresses_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Addresses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_Addresses_CityId] ON [Addresses] ([CityId]);

GO

CREATE INDEX [IX_Addresses_UserId] ON [Addresses] ([UserId]);

GO

CREATE INDEX [IX_Categories_ParentId] ON [Categories] ([ParentId]);

GO

CREATE INDEX [IX_Cities_ProvinceId] ON [Cities] ([ProvinceId]);

GO

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);

GO

CREATE INDEX [IX_UserActivationCodes_UserId] ON [UserActivationCodes] ([UserId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200815234342_init', N'3.1.7');

GO

CREATE TABLE [Wishlists] (
    [Id] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [ProductId] bigint NOT NULL,
    CONSTRAINT [PK_Wishlists] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Wishlists_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_Wishlists_UserId] ON [Wishlists] ([UserId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200816174804_Wishlist', N'3.1.7');

GO

CREATE TABLE [Brand] (
    [Id] bigint NOT NULL IDENTITY,
    [Name] nvarchar(255) NULL,
    CONSTRAINT [PK_Brand] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Products] (
    [Id] bigint NOT NULL IDENTITY,
    [Title] nvarchar(255) NULL,
    [Description] nvarchar(max) NULL,
    [CategoryId] bigint NOT NULL,
    [BrandId] bigint NOT NULL,
    [Characteristics] nvarchar(max) NULL,
    [ImageList] nvarchar(max) NULL,
    [Thubmnail] nvarchar(max) NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_Brand_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brand] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Baskets] (
    [Id] bigint NOT NULL IDENTITY,
    [ProductId] bigint NOT NULL,
    [Date] datetime2 NOT NULL,
    [Quantity] int NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [CharacteristicName] nvarchar(255) NULL,
    [CharacteristicValue] nvarchar(255) NULL,
    CONSTRAINT [PK_Baskets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Baskets_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_Wishlists_ProductId] ON [Wishlists] ([ProductId]);

GO

CREATE INDEX [IX_Baskets_ProductId] ON [Baskets] ([ProductId]);

GO

CREATE INDEX [IX_Products_BrandId] ON [Products] ([BrandId]);

GO

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);

GO

ALTER TABLE [Wishlists] ADD CONSTRAINT [FK_Wishlists_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200818002152_Added_Product', N'3.1.7');

GO

ALTER TABLE [Addresses] ADD [Title] nvarchar(255) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200818201030_Added_Address_Title', N'3.1.7');

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Addresses]') AND [c].[name] = N'Location');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Addresses] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Addresses] DROP COLUMN [Location];

GO

ALTER TABLE [Addresses] ADD [Lat] float NOT NULL DEFAULT 0.0E0;

GO

ALTER TABLE [Addresses] ADD [Lng] float NOT NULL DEFAULT 0.0E0;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200820090743_Address_Location_Lat_Lng', N'3.1.7');

GO

ALTER TABLE [Products] ADD [Properties] nvarchar(max) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200821105641_Product_Properties', N'3.1.7');

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'Image');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Categories] ALTER COLUMN [Image] nvarchar(max) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200822154521_Category_Image_Path', N'3.1.7');

GO

ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_Brand_BrandId];

GO

DROP TABLE [Baskets];

GO

ALTER TABLE [Brand] DROP CONSTRAINT [PK_Brand];

GO

EXEC sp_rename N'[Brand]', N'Brands';

GO

ALTER TABLE [Brands] ADD CONSTRAINT [PK_Brands] PRIMARY KEY ([Id]);

GO

CREATE TABLE [Orders] (
    [Id] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [OrrderDate] datetime2 NOT NULL,
    [TotalPrice] decimal(18,2) NOT NULL,
    [State] int NOT NULL,
    [AddressId] bigint NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_Addresses_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Addresses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Orders_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ProductPrices] (
    [Id] bigint NOT NULL IDENTITY,
    [ProductId] bigint NOT NULL,
    [CharacteristicValues] nvarchar(max) NULL,
    [Price] decimal(18,2) NOT NULL,
    [DiscountPrice] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_ProductPrices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductPrices_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ProductQuantities] (
    [Id] bigint NOT NULL IDENTITY,
    [ProductId] bigint NOT NULL,
    [CharacteristicValues] nvarchar(max) NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_ProductQuantities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductQuantities_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [OrderItems] (
    [Id] bigint NOT NULL IDENTITY,
    [ProductId] bigint NOT NULL,
    [OrderId] bigint NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [CharacteristicValues] nvarchar(max) NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Payments] (
    [Id] bigint NOT NULL IDENTITY,
    [OrderId] bigint NOT NULL,
    [PayToken] nvarchar(max) NULL,
    [UniqueId] nvarchar(max) NULL,
    [Amount] decimal(18,2) NOT NULL,
    [RegisterDate] datetime2 NOT NULL,
    [ReferenceNumber] nvarchar(100) NULL,
    [TrackingNumber] nvarchar(100) NULL,
    [State] int NOT NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);

GO

CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);

GO

CREATE INDEX [IX_Orders_AddressId] ON [Orders] ([AddressId]);

GO

CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);

GO

CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);

GO

CREATE INDEX [IX_ProductPrices_ProductId] ON [ProductPrices] ([ProductId]);

GO

CREATE INDEX [IX_ProductQuantities_ProductId] ON [ProductQuantities] ([ProductId]);

GO

ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brands] ([Id]) ON DELETE CASCADE;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200825000625_Quantity_Price_Order_Payment', N'3.1.7');

GO

