IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] uniqueidentifier NOT NULL,
        [Nome] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    CREATE TABLE [Produtos] (
        [Id] uniqueidentifier NOT NULL,
        [Nome] nvarchar(100) NOT NULL,
        [Descricao] nvarchar(500) NOT NULL,
        [Preco] decimal(18,2) NOT NULL,
        [Estoque] int NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Produtos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    CREATE TABLE [Pedidos] (
        [Id] uniqueidentifier NOT NULL,
        [ClienteId] uniqueidentifier NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CriadoEmUtc] datetime2 NOT NULL,
        [AtualizadoEmUtc] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Pedidos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pedidos_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    CREATE TABLE [ItensPedido] (
        [Id] uniqueidentifier NOT NULL,
        [ProdutoId] uniqueidentifier NOT NULL,
        [NomeProduto] nvarchar(100) NOT NULL,
        [PrecoUnitario] decimal(18,2) NOT NULL,
        [Quantidade] int NOT NULL,
        [PedidoId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ItensPedido] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ItensPedido_Pedidos_PedidoId] FOREIGN KEY ([PedidoId]) REFERENCES [Pedidos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ItensPedido_Produtos_ProdutoId] FOREIGN KEY ([ProdutoId]) REFERENCES [Produtos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Email', N'Nome') AND [object_id] = OBJECT_ID(N'[Clientes]'))
        SET IDENTITY_INSERT [Clientes] ON;
    EXEC(N'INSERT INTO [Clientes] ([Id], [Email], [Nome])
    VALUES (''0199a000-0000-7000-8000-000000000001'', N''ana.souza@exemplo.com'', N''Ana Souza''),
    (''0199a000-0000-7000-8000-000000000002'', N''bruno.lima@exemplo.com'', N''Bruno Lima''),
    (''0199a000-0000-7000-8000-000000000003'', N''carla.mendes@exemplo.com'', N''Carla Mendes'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Email', N'Nome') AND [object_id] = OBJECT_ID(N'[Clientes]'))
        SET IDENTITY_INSERT [Clientes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descricao', N'Estoque', N'Nome', N'Preco') AND [object_id] = OBJECT_ID(N'[Produtos]'))
        SET IDENTITY_INSERT [Produtos] ON;
    EXEC(N'INSERT INTO [Produtos] ([Id], [Descricao], [Estoque], [Nome], [Preco])
    VALUES (''0199a000-0000-7000-8000-000000000101'', N''Teclado ABNT2 com switches marrons.'', 15, N''Teclado mecânico'', 349.9),
    (''0199a000-0000-7000-8000-000000000102'', N''Mouse óptico 1600 DPI com receptor USB.'', 40, N''Mouse sem fio'', 89.9),
    (''0199a000-0000-7000-8000-000000000103'', N''Monitor Full HD IPS de 24 polegadas.'', 8, N''Monitor 24"'', 899.0),
    (''0199a000-0000-7000-8000-000000000104'', N''Headset com microfone e cancelamento de ruído.'', 20, N''Headset USB'', 199.5),
    (''0199a000-0000-7000-8000-000000000105'', N''Webcam 1080p com microfone embutido.'', 3, N''Webcam HD'', 159.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descricao', N'Estoque', N'Nome', N'Preco') AND [object_id] = OBJECT_ID(N'[Produtos]'))
        SET IDENTITY_INSERT [Produtos] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_Email] ON [Clientes] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ItensPedido_PedidoId_ProdutoId] ON [ItensPedido] ([PedidoId], [ProdutoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    CREATE INDEX [IX_ItensPedido_ProdutoId] ON [ItensPedido] ([ProdutoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    CREATE INDEX [IX_Pedidos_ClienteId] ON [Pedidos] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    CREATE INDEX [IX_Pedidos_CriadoEmUtc] ON [Pedidos] ([CriadoEmUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    CREATE INDEX [IX_Pedidos_Status] ON [Pedidos] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929123953_InicialCriacao'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929123953_InicialCriacao', N'10.0.12');
END;

COMMIT;
GO

