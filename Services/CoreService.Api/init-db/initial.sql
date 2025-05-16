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
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    MiddleName VARCHAR(100),
    UserId VARCHAR(255) NOT NULL,
    Department VARCHAR(500),
    CanSuperviseVKR BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE Students
(
    Id SERIAL PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    MiddleName VARCHAR(100),
    UserId VARCHAR(255) NOT NULL,
    GroupId INT,
    CONSTRAINT Group_FK FOREIGN KEY (GroupId) REFERENCES Groups (Id)
);

CREATE TABLE Consultants
(
    Id SERIAL PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    MiddleName VARCHAR(100),
    Contact VARCHAR(500) NOT NULL,
    UserId VARCHAR(255)
);

CREATE TABLE Themes
(
    Id SERIAL PRIMARY KEY,
    Title VARCHAR(50) NOT NULL,
    Description VARCHAR NOT NULL,
    Tags JSONB,
    Level VARCHAR(255) NOT NULL, 
    Department VARCHAR(500),
    IsArchived BOOLEAN NOT NULL DEFAULT FALSE,
    SuggestedBy VARCHAR(100), 
    Source VARCHAR(500) NOT NULL, 
    ConsultantId INT,
    SupervisorId INT,
    CreatedDate TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedDate TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT Lecturer_FK FOREIGN KEY (SupervisorId) REFERENCES Lecturers (Id),
    CONSTRAINT Consultant_FK FOREIGN KEY (ConsultantId) REFERENCES Consultants (Id)
);

CREATE TABLE Practices
(
    Id SERIAL PRIMARY KEY,
    StudentId INT NOT NULL,
    ConsultantId INT NOT NULL,
    SupervisorId INT,
    ThemeId INT NOT NULL,
    Type VARCHAR(255) NOT NULL, 
    FinalGrade VARCHAR(5),
    Status VARCHAR(50) NOT NULL,
    CreatedDate TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedDate TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT Student_FK FOREIGN KEY (StudentId) REFERENCES Students (Id),
    CONSTRAINT Theme_FK FOREIGN KEY (ThemeId) REFERENCES Themes (Id)
);
