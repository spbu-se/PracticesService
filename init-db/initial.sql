CREATE TABLE Groups
(
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(50) NOT NULL,
    Program VARCHAR(500) NOT NULL,
    Year SMALLINT NOT NULL
);

CREATE TABLE Lecturers
(
    Id SERIAL PRIMARY KEY,
    UserId UUID NOT NULL, -- Внешний идентификатор пользователя
    Department VARCHAR(255),
    CanSuperviseVKR BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE Students
(
    Id SERIAL PRIMARY KEY,
    UserId UUID NOT NULL, -- Внешний идентификатор пользователя
    GroupId INT NOT NULL,
    CONSTRAINT Group_FK FOREIGN KEY (GroupId) REFERENCES Groups (Id)
);

CREATE TABLE Consultants
(
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Contact VARCHAR(255) NOT NULL,
    UserId UUID -- Внешний идентификатор пользователя, может не быть
);

CREATE TABLE Themes
(
    Id SERIAL PRIMARY KEY,
    Title VARCHAR(50) NOT NULL,
    Description VARCHAR(500) NOT NULL,
    Tags JSONB,
    Department VARCHAR(500),
    IsArchived BOOLEAN NOT NULL DEFAULT FALSE,
    SuggestedBy VARCHAR(255) NOT NULL, 
    ConsultantId INT NOT NULL,
    SupervisorId INT NOT NULL,
    CreatedDate TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedDate TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT Lecturer_FK FOREIGN KEY (SupervisorId) REFERENCES Lecturers (Id),
    CONSTRAINT Consultant_FK FOREIGN KEY (ConsultantId) REFERENCES Consultants (Id)
);

CREATE TABLE Practices
(
    Id SERIAL PRIMARY KEY,
    StudentId INT NOT NULL,
    ThemeId INT NOT NULL,
    FinalGrade VARCHAR(5),
    Status VARCHAR(50) NOT NULL,
    CreatedDate TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedDate TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT Student_FK FOREIGN KEY (StudentId) REFERENCES Students (Id),
    CONSTRAINT Theme_FK FOREIGN KEY (ThemeId) REFERENCES Themes (Id)
);

INSERT INTO Lecturers (UserId, Department, CanSuperviseVKR) VALUES ('e5c49d19-89a1-4a67-8b7c-9b3c6b84e90d', 'Software Enginering', TRUE);
