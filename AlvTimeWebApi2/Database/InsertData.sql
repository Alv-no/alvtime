USE AlvDevDB
GO

insert into [dbo].[User]
values
('Ansatt En  ', 'ansatten@alv.no', 90, 4),
('Ansatt To  ', 'ansatto@alv.no', 42, 53),
('Ansatt Tre  ', 'ansattre@alv.no', 5, 53),
('Ansatt Fire  ', 'ansattfire@alv.no', 10, 10),
('Ansatt Fem  ', 'ansattfem@alv.no', 100, 34),
('Ansatt Seks  ', 'ansattseks@alv.no', 4, 31),
('Ansatt Syv  ', 'ansattsyv@alv.no', 49, 23),
('Ansatt Atte  ', 'ansattatte@alv.no', 75, 23),
('Ansatt Ni  ', 'ansattni@alv.no', 76, 12),
('Ansatt Ti  ', 'ansatti@alv.no', 85, 26)

insert into TaskFavorites
select id as UserId, 12 as TaskId
from [dbo].[User]

insert into TaskFavorites
select id as UserId, 13 as TaskId
from [dbo].[User]

insert into TaskFavorites
select id as UserId, 14 as TaskId
from [dbo].[User]

insert into TaskFavorites
select id as UserId, 15 as TaskId
from [dbo].[User]

insert into TaskFavorites
select id as UserId, 17 as TaskId
from [dbo].[User]

insert into TaskFavorites
select id as UserId, 8 as TaskId
from [dbo].[User]

insert into task
values
('Testleder              ','',	2,	1000.00, 0, 0, 'Datafabrikken'),
('Funksjonell arkitekt   ','',	2,	1000.00, 0, 0, 'SuperMat'),
('Teamleder              ','',	6,	1000.00, 0, 0, 'Film og TV'),
('Prosjektstotte         ','',	6,	1000.00, 0, 0, 'Rutsjebaner AS'),
('Testleder              ','',	3,	1000.00, 0, 0, 'Hummer og Kanari'),
('Testradgiver           ','',	3,	1000.00, 0, 0, 'SuperMat'),
('Cost controller        ','',	8,	1000.00, 0, 0, 'SuperMat'),
('Utvikler               ','',	7,	1000.00, 0, 0, 'Film og TV'),
('Seniorutvikler         ','',	1,	1000.00, 0, 0, 'Film og TV'),
('Juniorutvikler         ','',	1,	1000.00, 0, 0, 'Datafabrikken'),
('Sikkerhetstester       ','',	5,	1000.00, 0, 0, 'Rutsjebaner AS'),
('Interntid              ','',	9,	0.00,	 0, 0, 'Alv AS'),
('Ferie                  ','',	9,	0.00,	 0, 0, 'Alv AS'),
('Syk                    ','',	9,	0.00,	 0, 0, 'Alv AS'),
('Sykt barn              ','',	9,	0.00,	 0, 0, 'Alv AS'),
('Youtube serie          ','',	9,	0.00,	 0, 0, 'Alv AS'),
('AlvTimeUtvikling       ','',	9,	0.00,	 0, 0, 'Alv AS')

insert into [dbo].[Hours]
values
(1, 15, '12/12/2019 00:00:00', 346, 2019, 15),
(1, 12, '07/12/2019 00:00:00', 341, 2019, 5),
(2, 3,  '10/12/2019 00:00:00', 344, 2019, 6),
(3, 4,  '01/12/2019 00:00:00', 335, 2019, 12),
(4, 4,  '01/12/2019 00:00:00', 335, 2019, 12),
(5, 4,  '01/12/2019 00:00:00', 335, 2019, 12),
(6, 4,  '01/12/2019 00:00:00', 335, 2019, 12),
(7, 4,  '01/12/2019 00:00:00', 335, 2019, 12),
(8, 4,  '01/12/2019 00:00:00', 335, 2019, 12),
(9, 4,  '01/12/2019 00:00:00', 335, 2019, 12),
(10, 4, '01/12/2019 00:00:00', 335, 2019, 12),
(5, 4,  '01/12/2019 00:00:00', 335, 2019, 12),
(3, 4,  '01/12/2019 00:00:00', 335, 2019, 12),
(8, 4,  '01/12/2019 00:00:00', 335, 2019, 12)


insert into Project
values 
('MatAppen        ' ,1),
('LeverMatTilMeg  ' ,1),
('Sklier          ' ,2),
('Luksussmellen   ' ,3),
('Paradise Motell ' ,3),
('Disks n Chips   ' ,4),
('Hurramegrundt   ' ,2),
('Alv             ' ,5)

insert into Customer
values 
('SuperMat       '),                                                                                        
('Rutsjebaner AS '),                                                                                        
('Film og TV     '),                                                                                        
('Datafabrikken  '),                                                                                                                                                                               
('Alv            ')                                                                                        