-- E S T A N D A R - D E - N O M B R E S


-- #_ A E R O P U E R T O S _#
-- [Gentilicio en ingl�s] [CIUDAD en may�sculas] [idPais]-[idCiudad] Airport





-- ##########################################################
-- #                 NOMENCLATURA DEL C�DIGO DE VUELO       #
-- ##########################################################
-- El c�digo de vuelo sigue la siguiente estructura:
-- 
-- [Inicial de la aerol�nea] [ID de la aerol�nea] - 
-- [C�digo de 3 letras del pa�s de origen] - 
-- [C�digo de 3 letras del pa�s de destino] - 
-- [N�mero de vuelo con formato #XXXX]
-- 
-- Donde:
-- - La inicial de la aerol�nea es la primera letra del nombre de la aerol�nea.
-- - El ID de la aerol�nea es el n�mero asignado en la base de datos.
-- - El c�digo de 3 letras del pa�s proviene del listado estandarizado.
-- - El n�mero de vuelo es el ID del vuelo, rellenado con ceros a la izquierda hasta 4 d�gitos (#0001, #0999, #1530).
-- 
-- ##########################################################
-- #                         EJEMPLO                        #
-- ##########################################################
-- Un vuelo de **American Airlines (ID 1)** de **El Salvador (SLV)** a **Jap�n (JPN)**
-- con un ID de vuelo 25, tendr�a el siguiente c�digo:
--
-- **A1-SLV-JPN-#0025**
--
-- ##########################################################
-- #                  LISTADO DE C�DIGOS DE PA�SES          #
-- ##########################################################
-- Estos son los c�digos de 3 letras asignados a cada pa�s:
-- 
-- Argentina            - ARG
-- Belice               - BLZ
-- Brasil               - BRA
-- Canad�               - CAN
-- Chile                - CHL
-- Colombia             - COL
-- Costa Rica           - CRI
-- El Salvador          - SLV
-- Estados Unidos       - USA
-- Guatemala            - GTM
-- Honduras             - HND
-- M�xico               - MEX
-- Panam�               - PAN
-- Per�                 - PER
-- Puerto Rico          - PRI
-- Rep�blica Dominicana - DOM
-- Jap�n                - JPN
-- Alemania             - DEU
-- Espa�a               - ESP
-- Francia              - FRA
-- Portugal             - PRT
-- Reino Unido          - GBR
-- Suecia               - SWE
-- Australia            - AUS
-- Nueva Zelanda        - NZL
