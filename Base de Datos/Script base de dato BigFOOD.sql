CREATE DATABASE BigFOOD;
GO

USE BigFOOD;
GO

-- TABLA USUARIOS
CREATE TABLE Usuarios
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Login VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(100) NOT NULL,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    Estado BIT NOT NULL DEFAULT 1
);
GO

-- TABLA CLIENTES
CREATE TABLE Clientes
(
    CedulaLegal VARCHAR(20) PRIMARY KEY,
    TipoCedula VARCHAR(20) NOT NULL,
    NombreCompleto VARCHAR(150) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    Estado BIT NOT NULL DEFAULT 1,
    UsuarioId INT NOT NULL,

    CONSTRAINT UQ_Clientes_Email
        UNIQUE (Email),

    CONSTRAINT CK_Clientes_TipoCedula
        CHECK (TipoCedula IN ('FISICA','JURIDICA','DIMEX')),

    CONSTRAINT FK_Cliente_Usuario
        FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(Id)
);
GO

-- TABLA PRODUCTOS
CREATE TABLE Productos
(
    CodigoInterno INT IDENTITY(1,1) PRIMARY KEY,
    CodigoBarra VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(150) NOT NULL,
    PrecioVenta DECIMAL(18,2) NOT NULL,
    Descuento DECIMAL(18,2) NOT NULL DEFAULT 0,
    Impuesto DECIMAL(18,2) NOT NULL DEFAULT 13,
    UnidadMedida VARCHAR(50) NOT NULL,
    PrecioCompra DECIMAL(18,2) NOT NULL,
    UsuarioId INT NOT NULL,
    Existencia INT NOT NULL DEFAULT 0,

    CONSTRAINT UQ_Productos_Descripcion
        UNIQUE (Descripcion),

    CONSTRAINT FK_Producto_Usuario
        FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(Id)
);
GO

-- TABLA FACTURAS
CREATE TABLE Facturas
(
    Numero INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    CedulaCliente VARCHAR(20) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    MontoDescuento DECIMAL(18,2) NOT NULL,
    MontoImpuesto DECIMAL(18,2) NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'ACTIVA',
    UsuarioId INT NOT NULL,
    TipoPago VARCHAR(20) NOT NULL,
    Condicion VARCHAR(20) NOT NULL,

    CONSTRAINT CK_Facturas_TipoPago
        CHECK (TipoPago IN ('EFECTIVO','TARJETA','SINPE MOVIL')),

    CONSTRAINT CK_Facturas_Condicion
        CHECK (Condicion IN ('CONTADO','CREDITO')),

    CONSTRAINT CK_Facturas_Estado
        CHECK (Estado IN ('PAGADA','PENDIENTE','ANULADA')),

    CONSTRAINT FK_Factura_Cliente
        FOREIGN KEY (CedulaCliente)
        REFERENCES Clientes(CedulaLegal),

    CONSTRAINT FK_Factura_Usuario
        FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(Id)
);
GO

-- TABLA DETALLE FACTURAS
CREATE TABLE Det_Facturas
(
    NumFactura INT NOT NULL,
    CodInterno INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    PorImp DECIMAL(18,2) NOT NULL,
    PorDescuento DECIMAL(18,2) NOT NULL,

    PRIMARY KEY (NumFactura, CodInterno),

    CONSTRAINT FK_DetFactura_Factura
        FOREIGN KEY (NumFactura)
        REFERENCES Facturas(Numero),

    CONSTRAINT FK_DetFactura_Producto
        FOREIGN KEY (CodInterno)
        REFERENCES Productos(CodigoInterno)
);
GO

-- TABLA CUENTAS POR COBRAR
CREATE TABLE CuentasPorCobrar
(
    NumFactura INT PRIMARY KEY,
    CedulaCliente VARCHAR(20) NOT NULL,
    FechaFactura DATETIME NOT NULL,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    MontoFactura DECIMAL(18,2) NOT NULL,
    UsuarioId INT NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'PENDIENTE',

    CONSTRAINT CK_CuentasPorCobrar_Estado
        CHECK (Estado IN ('PENDIENTE','PAGADO')),

    CONSTRAINT FK_CuentasPorCobrar_Cliente
        FOREIGN KEY (CedulaCliente)
        REFERENCES Clientes(CedulaLegal),

    CONSTRAINT FK_CuentasPorCobrar_Factura
        FOREIGN KEY (NumFactura)
        REFERENCES Facturas(Numero),

    CONSTRAINT FK_CuentasPorCobrar_Usuario
        FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(Id)
);
GO

-- TABLA BITACORA
CREATE TABLE Bitacora
(
    IdBitacora INT IDENTITY(1,1) PRIMARY KEY,
    Tabla VARCHAR(50) NOT NULL,
    UsuarioId INT NOT NULL,
    Maquina VARCHAR(100) NOT NULL,
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    TipoMov VARCHAR(20) NOT NULL,
    Registro VARCHAR(100) NOT NULL,

    CONSTRAINT FK_Bitacora_Usuario
        FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(Id)
);
GO

-- USUARIO DE PRUEBA
INSERT INTO Usuarios
(
    Login,
    Password
)
VALUES
(
    'admin',
    '123'
);
GO


