-- Update the scraperrun table to ensure string columns can be NULL
ALTER TABLE `webstraction_db`.`scraperrun` 
  MODIFY COLUMN `errormessage` TEXT NULL DEFAULT NULL,
  MODIFY COLUMN `elapsedtime` VARCHAR(50) NULL DEFAULT NULL;

-- Update the scrapermetric table to ensure runId can be NULL
ALTER TABLE `webstraction_db`.`scrapermetric` 
  MODIFY COLUMN `runid` VARCHAR(36) NULL DEFAULT NULL;

-- Update the logentry table to ensure runId can be NULL
ALTER TABLE `webstraction_db`.`logentry` 
  MODIFY COLUMN `runid` VARCHAR(36) NULL DEFAULT NULL;

-- Update the processeddocument table to ensure runId can be NULL
ALTER TABLE `webstraction_db`.`processeddocument` 
  MODIFY COLUMN `runid` VARCHAR(36) NULL DEFAULT NULL;

-- Update the pipelinemetric table to ensure runId can be NULL
ALTER TABLE `webstraction_db`.`pipelinemetric` 
  MODIFY COLUMN `runid` VARCHAR(36) NULL DEFAULT NULL;
