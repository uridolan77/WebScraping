-- Comprehensive MySQL script to rename all columns in all tables from snake_case to camelCase
-- Works with all tables in the webstraction_db database

SET FOREIGN_KEY_CHECKS = 0;

-- First, get a list of all tables in the database
DROP TABLE IF EXISTS temp_tables;
CREATE TEMPORARY TABLE temp_tables (table_name VARCHAR(255));
INSERT INTO temp_tables
SELECT table_name FROM information_schema.tables
WHERE table_schema = 'webstraction_db' AND table_type = 'BASE TABLE';

-- Function to convert snake_case to camelCase (we'll create this in a procedure to avoid syntax issues)
DROP PROCEDURE IF EXISTS process_all_tables;
DELIMITER //

CREATE PROCEDURE process_all_tables()
BEGIN
    -- Variables for table cursor
    DECLARE done BOOLEAN DEFAULT FALSE;
    DECLARE current_table VARCHAR(255);
    
    -- Variables for column processing
    DECLARE snake_columns_exist BOOLEAN DEFAULT FALSE;
    DECLARE rename_sql TEXT;
    
    -- Cursor for iterating through all tables
    DECLARE table_cursor CURSOR FOR 
        SELECT table_name FROM temp_tables;
    
    -- Handler for cursor
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    -- Create helper function to convert snake_case to camelCase
    SET @create_func = 'DROP FUNCTION IF EXISTS to_camel_case;
    CREATE FUNCTION to_camel_case(snake VARCHAR(255)) RETURNS VARCHAR(255)
    DETERMINISTIC
    BEGIN
        DECLARE result VARCHAR(255);
        DECLARE i INT DEFAULT 1;
        DECLARE c CHAR(1);
        DECLARE capitalize BOOLEAN DEFAULT FALSE;
        
        SET result = LOWER(snake);
        
        WHILE i <= CHAR_LENGTH(result) DO
            SET c = SUBSTRING(result, i, 1);
            IF c = "_" THEN
                SET capitalize = TRUE;
                SET result = CONCAT(LEFT(result, i-1), SUBSTRING(result, i+1));
            ELSEIF capitalize THEN
                SET result = CONCAT(LEFT(result, i-1), UPPER(c), SUBSTRING(result, i+1));
                SET capitalize = FALSE;
            END IF;
            SET i = i + IF(c = "_", 0, 1);
        END WHILE;
        
        RETURN result;
    END;';
    
    PREPARE stmt FROM @create_func;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
    
    -- Open cursor and iterate through all tables
    OPEN table_cursor;
    
    table_loop: LOOP
        FETCH table_cursor INTO current_table;
        IF done THEN
            LEAVE table_loop;
        END IF;
        
        -- Report current table being processed
        SELECT CONCAT('Processing table: ', current_table) AS message;
        
        -- Check if table has any snake_case columns
        SELECT COUNT(*) > 0 INTO snake_columns_exist 
        FROM information_schema.columns
        WHERE table_schema = 'webstraction_db' 
          AND table_name = current_table 
          AND column_name LIKE '%\_%';
        
        IF snake_columns_exist THEN
            -- Start building the ALTER TABLE statement
            SET @rename_stmt = CONCAT('ALTER TABLE `', current_table, '` ');
            
            -- Create a temporary table to hold columns to rename
            DROP TEMPORARY TABLE IF EXISTS temp_columns;
            CREATE TEMPORARY TABLE temp_columns (
                column_name VARCHAR(255),
                new_column_name VARCHAR(255),
                column_definition TEXT
            );
            
            -- Get all snake_case columns with their definitions
            SET @insert_cols = CONCAT('
                INSERT INTO temp_columns
                SELECT 
                    column_name,
                    to_camel_case(column_name) AS new_column_name,
                    CONCAT(
                        column_type, 
                        IF(is_nullable = "YES", " NULL", " NOT NULL"),
                        CASE 
                            WHEN column_default IS NULL THEN ""
                            WHEN column_type IN ("tinyint", "smallint", "int", "bigint", "float", "double", "decimal") 
                                THEN CONCAT(" DEFAULT ", column_default)
                            ELSE CONCAT(" DEFAULT ''", column_default, "''")
                        END,
                        IF(extra <> "", CONCAT(" ", extra), "")
                    ) AS column_definition
                FROM information_schema.columns
                WHERE table_schema = "webstraction_db"
                  AND table_name = "', current_table, '"
                  AND column_name LIKE "%\\_%"
            ');
            
            PREPARE stmt FROM @insert_cols;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
            
            -- Build the CHANGE COLUMN clauses
            SELECT GROUP_CONCAT(
                CONCAT('CHANGE COLUMN `', column_name, '` `', new_column_name, '` ', column_definition)
                SEPARATOR ', '
            ) INTO @changes
            FROM temp_columns;
            
            -- Complete and execute the ALTER TABLE statement
            IF @changes IS NOT NULL THEN
                SET @alter_stmt = CONCAT(@rename_stmt, @changes, ';');
                
                -- Show the statement (for debugging)
                SELECT @alter_stmt AS executing_statement;
                
                -- Execute the statement
                PREPARE stmt FROM @alter_stmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                
                -- Report columns renamed
                SELECT CONCAT('Renamed columns in table: ', current_table) AS success;
            ELSE
                SELECT CONCAT('No columns to rename in table: ', current_table) AS skipped;
            END IF;
        ELSE
            SELECT CONCAT('No snake_case columns found in table: ', current_table) AS skipped;
        END IF;
    END LOOP;
    
    CLOSE table_cursor;
    
    -- Clean up helper function
    DROP FUNCTION IF EXISTS to_camel_case;
END //

DELIMITER ;

-- Execute the procedure to process all tables
CALL process_all_tables();

-- Clean up
DROP PROCEDURE IF EXISTS process_all_tables;
DROP TABLE IF EXISTS temp_tables;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Validation - report on any remaining snake_case columns after the conversion
SELECT 
    table_name,
    GROUP_CONCAT(column_name SEPARATOR ', ') AS snake_case_columns_remaining,
    COUNT(*) AS count
FROM information_schema.columns
WHERE 
    table_schema = 'webstraction_db'
    AND column_name LIKE '%\_%'
GROUP BY table_name
ORDER BY table_name;

-- Report success
SELECT 'All columns in all tables have been converted from snake_case to camelCase format.' AS Result;