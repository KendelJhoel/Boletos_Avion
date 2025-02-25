-- E S T A N D A R - D E - N O M B R E S


-- #_ A E R O P U E R T O S _#
-- [Gentilicio en inglés] [CIUDAD en mayúsculas] [idPais]-[idCiudad] Airport





-- ##########################################################
-- #                 NOMENCLATURA DEL CÓDIGO DE VUELO       #
-- ##########################################################
-- El código de vuelo sigue la siguiente estructura:
-- 
-- [Inicial de la aerolínea] [ID de la aerolínea] - 
-- [Código de 3 letras del país de origen] - 
-- [Código de 3 letras del país de destino] - 
-- [Número de vuelo con formato #XXXX]
-- 
-- Donde:
-- - La inicial de la aerolínea es la primera letra del nombre de la aerolínea.
-- - El ID de la aerolínea es el número asignado en la base de datos.
-- - El código de 3 letras del país proviene del listado estandarizado.
-- - El número de vuelo es el ID del vuelo, rellenado con ceros a la izquierda hasta 4 dígitos (#0001, #0999, #1530).
-- 
-- ##########################################################
-- #                         EJEMPLO                        #
-- ##########################################################
-- Un vuelo de **American Airlines (ID 1)** de **El Salvador (SLV)** a **Japón (JPN)**
-- con un ID de vuelo 25, tendría el siguiente código:
--
-- **A1-SLV-JPN-#0025**
--
-- ##########################################################
-- #                  LISTADO DE CÓDIGOS DE PAÍSES          #
-- ##########################################################
-- Estos son los códigos de 3 letras asignados a cada país:
-- 
-- Argentina            - ARG
-- Belice               - BLZ
-- Brasil               - BRA
-- Canadá               - CAN
-- Chile                - CHL
-- Colombia             - COL
-- Costa Rica           - CRI
-- El Salvador          - SLV
-- Estados Unidos       - USA
-- Guatemala            - GTM
-- Honduras             - HND
-- México               - MEX
-- Panamá               - PAN
-- Perú                 - PER
-- Puerto Rico          - PRI
-- República Dominicana - DOM
-- Japón                - JPN
-- Alemania             - DEU
-- España               - ESP
-- Francia              - FRA
-- Portugal             - PRT
-- Reino Unido          - GBR
-- Suecia               - SWE
-- Australia            - AUS
-- Nueva Zelanda        - NZL
