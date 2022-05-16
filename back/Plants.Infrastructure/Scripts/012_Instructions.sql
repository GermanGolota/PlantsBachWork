CREATE OR REPLACE VIEW instruction_v AS (
  SELECT
    i.id,
    i.plant_group_id,
    i.title,
    i.description,
    i.instruction_text,
    c.image IS NOT NULL AS has_cover
  FROM
    plant_caring_instruction i
  LEFT JOIN instruction_to_cover c ON c.instruction_id = i.id);

CREATE OR REPLACE FUNCTION search_instructions (groupId int, instructionTitle text, instructionDescription text)
  RETURNS TABLE (
    id int,
    title text,
    description text,
    has_cover boolean)
  SECURITY DEFINER
  AS $$
BEGIN
  RETURN QUERY (
    SELECT
      i.id, i.title, i.description, i.has_cover
    FROM instruction_v i
    WHERE
      plant_group_id = groupId
      AND (instructionTitle IS NULL
        OR to_tsvector(i.title) @@ to_tsquery(instructionTitle))
    AND (instructionDescription IS NULL
      OR to_tsvector(i.description) @@ to_tsquery(instructionDescription)));
END
$$
LANGUAGE plpgsql;

--producer
CREATE OR REPLACE FUNCTION create_instruction (groupId int, instructionText text, instructionTitle text, instructionDescription text, coverImage bytea)
  RETURNS int
  AS $$
DECLARE
  instructionId int;
BEGIN
  INSERT INTO plant_caring_instruction (instruction_text, plant_group_id, title, description)
    VALUES (instructionText, groupId, instructionTitle, instructionDescription)
  RETURNING
    id INTO instructionId;
  IF coverImage IS NOT NULL THEN
    INSERT INTO instruction_to_cover (instruction_id, image)
      VALUES (instructionId, coverImage);
  END IF;
  RETURN instructionId;
END
$$
LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE edit_instruction (instructionId int, groupId int, instructionText text, instructionTitle text, instructionDescription text, coverImage bytea)
SECURITY DEFINER
AS $$
BEGIN
  UPDATE
    plant_caring_instruction
  SET
    plant_group_id = groupId,
    instruction_text = instructionText,
    title = instructionTitle,
    description = instructionDescription
  WHERE
    id = instructionId;
  IF coverImage IS NOT NULL THEN
    UPDATE
      instruction_to_cover
    SET
      image = coverImage
    WHERE
      instruction_id = instructionId;
  END IF;
END
$$
LANGUAGE plpgsql;

