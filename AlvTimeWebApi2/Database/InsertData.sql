USE AlvDevDB
GO

insert into [dbo].[Customer]
values 
('SuperMat'),                                                                                        
('Rutsjebaner AS'),                                                                                        
('Film og TV'),                                                                                        
('Datafabrikken'),                                                                                                                                                                               
('Alv')       

insert into [dbo].[Project]
values 
('MatAppen' ,1),
('LeverMatTilMeg' ,1),
('Sklier' ,2),
('Luksussmellen' ,3),
('Paradise Motell' ,3),
('Disks n Chips' ,4),
('Hurramegrundt' ,2),
('Alv' ,5)

insert into [dbo].[User]
values
('Ansatt En', 'ansatten@alv.no'),
('Ansatt To', 'ansatto@alv.no'),
('Ansatt Tre', 'ansattre@alv.no'),
('Ansatt Fire', 'ansattfire@alv.no'),
('Ansatt Fem', 'ansattfem@alv.no'),
('Ansatt Seks', 'ansattseks@alv.no'),
('Ansatt Syv', 'ansattsyv@alv.no'),
('Ansatt Atte', 'ansattatte@alv.no'),
('Ansatt Ni', 'ansattni@alv.no'),
('Ansatt Ti', 'ansatti@alv.no')

insert into [dbo].[TaskFavorites]
select id as UserId, 12 as TaskId
from [dbo].[User]

insert into [dbo].[TaskFavorites]
select id as UserId, 13 as TaskId
from [dbo].[User]

insert into [dbo].[TaskFavorites]
select id as UserId, 14 as TaskId
from [dbo].[User]

insert into [dbo].[TaskFavorites]
select id as UserId, 15 as TaskId
from [dbo].[User]

insert into [dbo].[TaskFavorites]
select id as UserId, 17 as TaskId
from [dbo].[User]

insert into [dbo].[TaskFavorites]
select id as UserId, 8 as TaskId
from [dbo].[User]

insert into [dbo].[Task]
values
('Testleder', '', 2, 0, 0),
('Funksjonell arkitekt', '', 2, 0, 0),
('Teamleder', '', 6, 0, 0),
('Prosjektstotte', '', 6, 0, 0),
('Testleder', '', 3, 0, 0),
('Testradgiver', '', 3, 0, 0),
('Testradgiver', '', 6, 0, 0),
('Testradgiver', '', 4, 0, 0),
('Cost controller', '',	8, 0, 0),
('Cost controller', '',	4, 0, 0),
('Cost controller', '',	5, 0, 0),
('Utvikler', '', 7, 0, 0),
('Utvikler', '', 4, 0, 0),
('Utvikler', '', 5, 0, 0),
('Seniorutvikler', '', 1, 0, 0),
('Juniorutvikler', '', 1, 0, 0),
('Sikkerhetstester', '',2, 0, 0),
('Interntid' ,'', 8, 0, 0),
('Ferie', '', 8, 0, 0),
('Syk', '',	8, 0, 0),
('Sykt barn', '', 8, 0, 0),
('Youtube serie', '', 8, 0, 0),
('AlvTimeUtvikling', '', 8, 0, 0)

insert into [dbo].[Hours]
values
(1, 7.5, '12/12/2019 00:00:00', 346, 2019, 13, 0),
(1, 3.5, '07/12/2019 00:00:00', 341, 2019, 5, 0),
(2, 3,  '10/12/2019 00:00:00', 344, 2019, 6, 0),
(3, 4,  '01/12/2019 00:00:00', 335, 2019, 8, 0),
(4, 4,  '01/12/2019 00:00:00', 335, 2019, 10, 0),
(5, 4,  '01/12/2019 00:00:00', 335, 2019, 11, 0),
(6, 4,  '01/12/2019 00:00:00', 335, 2019, 12, 0),
(7, 4,  '01/12/2019 00:00:00', 335, 2019, 3, 0),
(8, 4,  '01/12/2019 00:00:00', 335, 2019, 2, 0),
(9, 4,  '01/12/2019 00:00:00', 335, 2019, 9, 0),
(9, 4,  '01/12/2019 00:00:00', 335, 2019, 5, 0),
(10, 4, '01/12/2019 00:00:00', 335, 2019, 7, 0),
(5, 4,  '01/12/2019 00:00:00', 335, 2019, 4, 0),
(3, 4,  '01/12/2019 00:00:00', 335, 2019, 1, 0),
(8, 4,  '01/12/2019 00:00:00', 335, 2019, 1, 0)

insert into [dbo].[HourRate]
values
('12/12/2019 00:00:00', 1000, 1),
('07/12/2019 00:00:00', 200, 2),
('10/12/2019 00:00:00', 1450, 3),
('01/12/2019 00:00:00', 1250, 4),
('01/12/2019 00:00:00', 900, 5), 
('12/12/2019 00:00:00', 1000, 6),
('07/12/2019 00:00:00', 200, 7),
('10/12/2019 00:00:00', 1450, 8),
('01/12/2019 00:00:00', 1250, 9),
('01/12/2019 00:00:00', 900, 10),   
('12/12/2019 00:00:00', 1000, 11),
('07/12/2019 00:00:00', 200, 12),
('10/12/2019 00:00:00', 1250, 13)  