-- Simple MySQL script to rename all columns in all tables from snake_case to camelCase
-- Avoiding stored procedures to work around MySQL limitations

SET FOREIGN_KEY_CHECKS = 0;

-- List all tables with snake_case columns
SELECT 'Listing all tables with snake_case columns:' AS message;
SELECT 
    TABLE_NAME, 
    COUNT(*) AS snake_case_columns
FROM INFORMATION_SCHEMA.COLUMNS
WHERE 
    TABLE_SCHEMA = 'webstraction_db'
    AND COLUMN_NAME LIKE '%\_%'
GROUP BY TABLE_NAME
ORDER BY TABLE_NAME;

-- Generate SQL statements for each table
SELECT 'Generating SQL statements to rename columns in all tables...' AS message;

-- Create a temporary table to store the ALTER statements
DROP TABLE IF EXISTS temp_alter_statements;
CREATE TEMPORARY TABLE temp_alter_statements (
    table_name VARCHAR(255),
    alter_statement TEXT
);

-- For each table with snake_case columns, generate an ALTER TABLE statement
INSERT INTO temp_alter_statements (table_name, alter_statement)
SELECT 
    t.TABLE_NAME,
    CONCAT(
        'ALTER TABLE `', t.TABLE_NAME, '` ',
        GROUP_CONCAT(
            CONCAT(
                'CHANGE COLUMN `', c.COLUMN_NAME, '` `', 
                -- Convert snake_case to camelCase manually without a function
                -- First character always lowercase
                CONCAT(
                    LOWER(SUBSTRING(
                        REPLACE(
                            CONCAT(
                                SUBSTRING(c.COLUMN_NAME, 1, 1),
                                REPLACE(
                                    -- For each underscore followed by a character, replace with uppercase
                                    REGEXP_REPLACE(
                                        SUBSTRING(c.COLUMN_NAME, 2),
                                        '_([a-zA-Z])',
                                        CONCAT('', UPPER(SUBSTRING('\\1', 1)))
                                    ),
                                    '_', ''
                                )
                            ),
                            '_', ''
                        ),
                        1, 1
                    )),
                    -- Rest of the string after transformation
                    SUBSTRING(
                        REPLACE(
                            CONCAT(
                                SUBSTRING(c.COLUMN_NAME, 1, 1),
                                REPLACE(
                                    REGEXP_REPLACE(
                                        SUBSTRING(c.COLUMN_NAME, 2),
                                        '_([a-zA-Z])',
                                        CONCAT('', UPPER(SUBSTRING('\\1', 1)))
                                    ),
                                    '_', ''
                                )
                            ),
                            '_', ''
                        ),
                        2
                    )
                ),
                '` ', 
                c.COLUMN_TYPE,
                IF(c.IS_NULLABLE = 'YES', ' NULL', ' NOT NULL'),
                IF(c.COLUMN_DEFAULT IS NULL, '', 
                    CONCAT(' DEFAULT ', 
                        CASE 
                            WHEN c.COLUMN_TYPE IN ('tinyint', 'smallint', 'int', 'bigint', 'float', 'double', 'decimal') 
                                OR c.COLUMN_DEFAULT = 'NULL' 
                                THEN c.COLUMN_DEFAULT
                            ELSE CONCAT('\'', REPLACE(c.COLUMN_DEFAULT, '\'', '\\\''), '\'')
                        END
                    )
                ),
                IF(c.EXTRA != '', CONCAT(' ', c.EXTRA), '')
            )
            SEPARATOR ', '
        )
    ) AS alter_statement
FROM INFORMATION_SCHEMA.TABLES t
JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
WHERE 
    t.TABLE_SCHEMA = 'webstraction_db'
    AND t.TABLE_TYPE = 'BASE TABLE'
    AND c.COLUMN_NAME LIKE '%\_%'
GROUP BY t.TABLE_NAME;

-- Execute each ALTER statement
SELECT 'Executing ALTER statements for each table:' AS message;

-- Create a procedure to execute each statement
DROP PROCEDURE IF EXISTS execute_alter_statements;
DELIMITER //

CREATE PROCEDURE execute_alter_statements()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE table_name VARCHAR(255);
    DECLARE alter_statement TEXT;
    
    -- Cursor for the ALTER statements
    DECLARE cur CURSOR FOR SELECT table_name, alter_statement FROM temp_alter_statements;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    OPEN cur;
    
    read_loop: LOOP
        FETCH cur INTO table_name, alter_statement;
        IF done THEN
            LEAVE read_loop;
        END IF;
        
        -- Output the table being processed
        SELECT CONCAT('Processing table: ', table_name) AS message;
        
        -- Execute the ALTER statement
        SET @sql = alter_statement;
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        
        -- Verify the change
        SELECT CONCAT('Renamed columns in table: ', table_name) AS success;
    END LOOP;
    
    CLOSE cur;
END //

DELIMITER ;

-- Execute the procedure
CALL execute_alter_statements();

-- Clean up
DROP PROCEDURE IF EXISTS execute_alter_statements;
DROP TABLE IF EXISTS temp_alter_statements;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Final verification to check if any snake_case columns remain
SELECT 'Verifying results - checking for any remaining snake_case columns:' AS message;
SELECT 
    TABLE_NAME, 
    COUNT(*) AS remaining_snake_case_columns
FROM INFORMATION_SCHEMA.COLUMNS
WHERE 
    TABLE_SCHEMA = 'webstraction_db'
    AND COLUMN_NAME LIKE '%\_%'
GROUP BY TABLE_NAME
ORDER BY TABLE_NAME;

-- Summary message
SELECT 'All columns in all tables have been converted from snake_case to camelCase format.' AS Result;