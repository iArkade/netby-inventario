-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 24-09-2025 a las 17:27:45
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12


CREATE DATABASE IF NOT EXISTS `gestion_inventario`;
USE `gestion_inventario`;

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `gestion_inventario`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `productos`
--

CREATE TABLE `productos` (
  `Id` int(11) NOT NULL,
  `Name` varchar(200) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `Category` varchar(100) NOT NULL,
  `ImageUrl` varchar(500) DEFAULT NULL,
  `Price` decimal(18,2) NOT NULL,
  `Stock` int(11) NOT NULL DEFAULT 0 CHECK (`Stock` >= 0),
  `CreatedAt` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `productos`
--

INSERT INTO `productos` (`Id`, `Name`, `Description`, `Category`, `ImageUrl`, `Price`, `Stock`, `CreatedAt`) VALUES
(1, 'Laptop HP Pavilion', 'Laptop para oficina y gaming', 'Electronicos', NULL, 899.99, 6, '2025-09-23 16:20:20'),
(2, 'Mouse Inalambrico Logitech', 'Mouse ergonomico inalambrico', 'Electronicos', '', 25.99, 50, '2025-09-23 16:20:20'),
(4, 'Monitor Samsung 24\"', 'Monitor Full HD para oficina', 'Electronicos', NULL, 199.99, 10, '2025-09-23 16:20:20');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `transacciones`
--

CREATE TABLE `transacciones` (
  `Id` int(11) NOT NULL,
  `TransactionDate` timestamp NOT NULL DEFAULT current_timestamp(),
  `TransactionType` varchar(10) NOT NULL CHECK (`TransactionType` in ('Purchase','Sale')),
  `ProductId` int(11) NOT NULL,
  `ProductName` varchar(200) NOT NULL,
  `Quantity` int(11) NOT NULL CHECK (`Quantity` > 0),
  `UnitPrice` decimal(18,2) NOT NULL,
  `TotalPrice` decimal(18,2) NOT NULL,
  `Details` varchar(500) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `transacciones`
--

INSERT INTO `transacciones` (`Id`, `TransactionDate`, `TransactionType`, `ProductId`, `ProductName`, `Quantity`, `UnitPrice`, `TotalPrice`, `Details`) VALUES
(1, '2025-09-23 16:20:20', 'Purchase', 1, 'Laptop HP Pavilion', 7, 800.00, 5600.00, 'Compra inicial de inventario'),
(2, '2025-09-23 16:20:20', 'Sale', 2, 'Mouse Inalambrico Logitech', 5, 25.99, 129.95, 'Venta a cliente corporativo'),
(3, '2025-09-23 16:20:20', 'Purchase', 2, 'Mouse Inalambrico Logitech', 20, 20.00, 400.00, 'Reposici??n de stock');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `productos`
--
ALTER TABLE `productos`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_Products_Category` (`Category`),
  ADD KEY `IX_Products_Name` (`Name`);

--
-- Indices de la tabla `transacciones`
--
ALTER TABLE `transacciones`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_Transactions_ProductId` (`ProductId`),
  ADD KEY `IX_Transactions_Date` (`TransactionDate`),
  ADD KEY `IX_Transactions_Type` (`TransactionType`),
  ADD KEY `IX_Transactions_ProductName` (`ProductName`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `productos`
--
ALTER TABLE `productos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;

--
-- AUTO_INCREMENT de la tabla `transacciones`
--
ALTER TABLE `transacciones`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `transacciones`
--
ALTER TABLE `transacciones`
  ADD CONSTRAINT `transacciones_ibfk_1` FOREIGN KEY (`ProductId`) REFERENCES `productos` (`Id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
