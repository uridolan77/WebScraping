-- MySQL script to convert all field names in all tables from snake_case to camelCase
-- This script will detect and rename fields automatically for the entire database

-- Disable foreign key checks for the operations
SET FOREIGN_KEY_CHECKS = 0;

-- First, create the function to convert snake_case to camelCase
DROP FUNCTION IF EXISTS ToCamelCase;

DELIMITER //
CREATE FUNCTION ToCamelCase(input_string VARCHAR(255)) 
RETURNS VARCHAR(255)
DETERMINISTIC
BEGIN
    DECLARE result VARCHAR(255) DEFAULT "";
    DECLARE capitalize_next BOOLEAN DEFAULT FALSE;
    DECLARE i INT DEFAULT 1;
    DECLARE char_at VARCHAR(1);
    
    SET result = LOWER(input_string);
    
    WHILE i <= CHAR_LENGTH(result) DO
        SET char_at = SUBSTRING(result, i, 1);
        
        IF char_at = "_" THEN
            SET capitalize_next = TRUE;
        ELSE
            IF capitalize_next THEN
                SET result = CONCAT(
                    SUBSTRING(result, 1, i-1),
                    UPPER(char_at),
                    SUBSTRING(result, i+1)
                );
                SET capitalize_next = FALSE;
            END IF;
        END IF;
        
        SET i = i + 1;
    END WHILE;
    
    -- Remove all underscores
    SET result = REPLACE(result, "_", "");
    
    RETURN result;
END //
DELIMITER ;

-- Now create the procedure that uses the function
DROP PROCEDURE IF EXISTS ConvertToCamelCase;

DELIMITER //
CREATE PROCEDURE ConvertToCamelCase()
BEGIN
    -- Variables for cursor
    DECLARE done INT DEFAULT FALSE;
    DECLARE table_name VARCHAR(255);
    DECLARE column_name VARCHAR(255);
    DECLARE new_column_name VARCHAR(255);
    DECLARE column_type VARCHAR(255);
    DECLARE column_null VARCHAR(10);
    DECLARE column_default TEXT;
    DECLARE column_extra VARCHAR(255);
    DECLARE sql_stmt TEXT;
    
    -- Cursor for iterating over all columns in all tables
    DECLARE column_cursor CURSOR FOR
        SELECT 
            TABLE_NAME, 
            COLUMN_NAME,
            COLUMN_TYPE,
            IS_NULLABLE,
            COLUMN_DEFAULT,
            EXTRA
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE 
            TABLE_SCHEMA = 'webstraction_db'
            -- Only process columns with underscore in name (snake_case)
            AND COLUMN_NAME LIKE '%\_%';
    
    -- Handler for cursor
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    -- Open cursor and iterate through all columns
    OPEN column_cursor;
    
    column_loop: LOOP
        FETCH column_cursor INTO table_name, column_name, column_type, column_null, column_default, column_extra;
        
        IF done THEN
            LEAVE column_loop;
        END IF;
        
        -- Generate camelCase column name using the pre-created function
        SET new_column_name = ToCamelCase(column_name);
        
        -- Print table and column information (for debugging)
        SELECT CONCAT('Converting ', table_name, '.', column_name, ' to ', new_column_name) AS message;
        
        -- Prepare SQL statement for renaming the column
        SET sql_stmt = CONCAT(
            'ALTER TABLE `', table_name, '` ',
            'CHANGE COLUMN `', column_name, '` `', new_column_name, '` ',
            column_type, ' ',
            IF(column_null = 'YES', 'NULL', 'NOT NULL'), ' ',
            IF(column_default IS NULL, '', 
               CONCAT('DEFAULT ', 
                     CASE 
                         WHEN column_default = 'NULL' THEN 'NULL'
                         WHEN column_type LIKE '%int%' OR column_type LIKE '%float%' OR column_type LIKE '%double%' OR column_type LIKE '%decimal%' THEN column_default
                         ELSE CONCAT('\'', column_default, '\'')
                     END)), ' ',
            column_extra
        );
        
        -- Remove any double spaces from the SQL statement
        SET sql_stmt = REPLACE(sql_stmt, '  ', ' ');
        
        -- Print SQL statement (for debugging)
        SELECT sql_stmt;
        
        -- Execute the SQL statement
        SET @alter_stmt = sql_stmt;
        PREPARE stmt FROM @alter_stmt;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END LOOP;
    
    -- Close cursor
    CLOSE column_cursor;
END //
DELIMITER ;

-- Execute the procedure
CALL ConvertToCamelCase();

-- Clean up - drop the function and procedure
DROP PROCEDURE IF EXISTS ConvertToCamelCase;
DROP FUNCTION IF EXISTS ToCamelCase;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Print success message
SELECT 'All fields in all tables have been converted from snake_case to camelCase format.' AS Result;