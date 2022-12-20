--
-- File generated with SQLiteStudio v3.4.1 on Thu Dec 15 14:56:51 2022
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: allowances
DROP TABLE IF EXISTS allowances;

CREATE TABLE IF NOT EXISTS allowances (
    id    INTEGER PRIMARY KEY AUTOINCREMENT
                  NOT NULL,
    value NUMERIC NOT NULL
);


-- Table: insurances
DROP TABLE IF EXISTS insurances;

CREATE TABLE IF NOT EXISTS insurances (
    id       INTEGER PRIMARY KEY AUTOINCREMENT
                     NOT NULL,
    name     TEXT    UNIQUE
                     NOT NULL,
    interval INTEGER NOT NULL
                     DEFAULT (1) 
);


-- Table: insurances_to_residents
DROP TABLE IF EXISTS insurances_to_residents;

CREATE TABLE IF NOT EXISTS insurances_to_residents (
    id           INTEGER PRIMARY KEY AUTOINCREMENT
                         NOT NULL,
    insurance_id INTEGER NOT NULL
                         REFERENCES insurances (id) ON DELETE CASCADE,
    resident_id  INTEGER NOT NULL
                         REFERENCES residents (id) ON DELETE CASCADE,
    [primary]    INTEGER DEFAULT (0) 
                         NOT NULL,
    UNIQUE (
        insurance_id,
        resident_id
    )
    ON CONFLICT REPLACE
);


-- Table: products
DROP TABLE IF EXISTS products;

CREATE TABLE IF NOT EXISTS products (
    id     INTEGER PRIMARY KEY AUTOINCREMENT
                   NOT NULL,
    name   VARCHAR NOT NULL
                   UNIQUE,
    price  NUMERIC NOT NULL,
    amount INTEGER NOT NULL
);


-- Table: products_to_residents
DROP TABLE IF EXISTS products_to_residents;

CREATE TABLE IF NOT EXISTS products_to_residents (
    id          INTEGER PRIMARY KEY AUTOINCREMENT
                        NOT NULL,
    resident_id INTEGER NOT NULL,
    product_id  INTEGER NOT NULL,
    amount      INTEGER NOT NULL
                        DEFAULT 1,
    FOREIGN KEY (
        resident_id
    )
    REFERENCES residents (id) ON DELETE CASCADE,
    FOREIGN KEY (
        product_id
    )
    REFERENCES products (id) ON DELETE CASCADE,
    UNIQUE (
        resident_id,
        product_id
    )
    ON CONFLICT ROLLBACK
);


-- Table: residents
DROP TABLE IF EXISTS residents;

CREATE TABLE IF NOT EXISTS residents (
    id         INTEGER PRIMARY KEY AUTOINCREMENT
                       NOT NULL,
    first_name VARCHAR NOT NULL,
    last_name  VARCHAR NOT NULL,
    ssn        VARCHAR NOT NULL
                       UNIQUE,
    floor      INTEGER NOT NULL,
    birth_date TEXT    NOT NULL,
    entry_date TEXT    NOT NULL,
    exit_date  TEXT,
    image      VARCHAR
);


COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
