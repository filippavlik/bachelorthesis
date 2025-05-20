-- Remove conflicting tables
DROP TABLE IF EXISTS "Competitions" CASCADE;
DROP TABLE IF EXISTS "Fields" CASCADE;
DROP TABLE IF EXISTS "FilesPreviousDelegations" CASCADE;
DROP TABLE IF EXISTS "Matches" CASCADE;
DROP TABLE IF EXISTS "Teams" CASCADE;
DROP TABLE IF EXISTS "Transfers" CASCADE;
DROP TABLE IF EXISTS "Vetoes" CASCADE;
DROP TABLE IF EXISTS "TeamsMatches" CASCADE;
DROP TABLE IF EXISTS "StartingGameDates" CASCADE;

-- End of removing

CREATE TABLE "StartingGameDates" (
    game_date_id INT PRIMARY KEY CHECK (game_date_id = 1), -- Enforces only one row
    game_date DATE NOT NULL
);

CREATE TABLE "Competitions" (
    competition_id VARCHAR(15) NOT NULL,
    competition_name VARCHAR(100) NOT NULL,
    match_length INTEGER NOT NULL,
    league INTEGER NOT NULL
);
ALTER TABLE "Competitions" ADD CONSTRAINT pk_competitions PRIMARY KEY (competition_id);

CREATE TABLE "Fields" (
    field_id SERIAL NOT NULL,
    field_name VARCHAR(100) NOT NULL,
    field_address VARCHAR(200),
    latitude REAL NOT NULL,
    longitude REAL NOT NULL
);
ALTER TABLE "Fields" ADD CONSTRAINT pk_fields PRIMARY KEY (field_id);

CREATE TABLE "FilesPreviousDelegations" (
    file_id SERIAL NOT NULL,
    amount_of_matches INTEGER NOT NULL,
    delegations_from DATE NOT NULL,
    delegations_to DATE NOT NULL,
    file_uploaded_datetime TIMESTAMP NOT NULL,
    file_name VARCHAR(80) NOT NULL,
    file_uploaded_by INTEGER NOT NULL
);
ALTER TABLE "FilesPreviousDelegations" ADD CONSTRAINT pk_files_previous_delegations PRIMARY KEY (file_id);

CREATE TABLE "Matches" (
    match_id VARCHAR(20) NOT NULL,
    competition_id VARCHAR(15) NOT NULL,
    home_team_id VARCHAR(30) NOT NULL,
    away_team_id VARCHAR(30) NOT NULL,
    field_id INTEGER NOT NULL,
    match_date DATE NOT NULL,
    match_time TIME NOT NULL,
    post_match VARCHAR(20),
    pre_match VARCHAR(20),
    referee_id INTEGER,
    ar1_id INTEGER,
    ar2_id INTEGER,
    note VARCHAR(400),
    already_played BOOLEAN NOT NULL,
    locked BOOLEAN NOT NULL,
    last_changed_by VARCHAR(320) NOT NULL,
    last_changed TIMESTAMP NOT NULL
);
ALTER TABLE "Matches" ADD CONSTRAINT pk_matches PRIMARY KEY (match_id, competition_id,field_id);

CREATE TABLE "Teams" (
    team_id VARCHAR(30) NOT NULL,
    name VARCHAR(150) NOT NULL
);
ALTER TABLE "Teams" ADD CONSTRAINT pk_teams PRIMARY KEY (team_id);

CREATE TABLE "Transfers" (
    transfer_id SERIAL NOT NULL,
    referee_id INTEGER NOT NULL,
    previous_match_id VARCHAR(20),
    future_match_id VARCHAR(20),
    expected_departure TIMESTAMP NOT NULL,
    expected_arrival TIMESTAMP NOT NULL,
    from_home BOOLEAN NOT NULL,
    car BOOLEAN NOT NULL
);
ALTER TABLE "Transfers" ADD CONSTRAINT pk_transfers PRIMARY KEY (transfer_id);

CREATE TABLE "Vetoes" (
    veto_id SERIAL NOT NULL,
    competition_id VARCHAR(15) NOT NULL,
    team_id VARCHAR(30) NOT NULL,
    referee_id INTEGER NOT NULL,
    note VARCHAR(256)
);
ALTER TABLE "Vetoes" ADD CONSTRAINT pk_vetoes PRIMARY KEY (veto_id, competition_id, team_id);

CREATE TABLE "TeamsMatches" (
    team_id VARCHAR(30) NOT NULL,
    match_id VARCHAR(20) NOT NULL,
    competition_id VARCHAR(15) NOT NULL,
    field_id INTEGER NOT NULL
);
ALTER TABLE "TeamsMatches" ADD CONSTRAINT pk_teams_matches PRIMARY KEY (team_id, match_id, competition_id,field_id);

ALTER TABLE "Matches" ADD CONSTRAINT fk_matches_competitions FOREIGN KEY (competition_id) REFERENCES "Competitions" (competition_id) ON DELETE CASCADE;
ALTER TABLE "Matches" ADD CONSTRAINT fk_matches_fields FOREIGN KEY (field_id) REFERENCES "Fields" (field_id) ON DELETE CASCADE;

ALTER TABLE "Vetoes" ADD CONSTRAINT fk_vetoes_competitions FOREIGN KEY (competition_id) REFERENCES "Competitions" (competition_id) ON DELETE CASCADE;
ALTER TABLE "Vetoes" ADD CONSTRAINT fk_vetoes_teams FOREIGN KEY (team_id) REFERENCES "Teams" (team_id) ON DELETE CASCADE;

ALTER TABLE "TeamsMatches" ADD CONSTRAINT fk_teams_matches_teams FOREIGN KEY (team_id) REFERENCES "Teams" (team_id) ON DELETE CASCADE;
ALTER TABLE "TeamsMatches" ADD CONSTRAINT fk_teams_matches_matches FOREIGN KEY (match_id, competition_id,field_id) REFERENCES "Matches" (match_id, competition_id,field_id) ON DELETE CASCADE;

