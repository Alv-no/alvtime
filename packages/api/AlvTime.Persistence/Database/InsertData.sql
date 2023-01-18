USE AlvDevDB
GO

insert into [dbo].[Customer]
values 
('SuperMat', 'Alvvegen 34', 'Supermann', 'supermann@supermat.no', '81549300'),                                                                                        
('Rutsjebaner AS', 'Alvvegen 21', 'Willy', 'willy@rutsjebaner.no', '53153162'),                                                                                        
('Film og TV', 'Alvvegen 8', 'Halvstr?m', 'halvstrom@filmtv.no', '64136423'),                                                                                        
('Datafabrikken', 'Alvvegen 52', 'Roboto', 'roboto@data.no', '53154753'),                                                                                                                                                                               
('Alv', 'Alvvegen 27', 'Sjefen', 'sjefen@alv.no', '53178685')        

insert into [dbo].[Project]
values 
('MatAppen' ,1),
('LeverMatTilMeg' ,1),
('Sklier' ,2),
('Luksussmellen' ,3),
('Paradise Motell' ,3),
('Disks n Chips' ,4),
('Hurramegrundt' ,2),
('Alv' ,5),
('Fravær', 5)

insert into [dbo].[User]
values
('Ansatt En', 'ansatten@alv.no', '2019-08-01', NULL, 1),
('Ansatt To', 'ansatto@alv.no', '2019-09-01', NULL, 2),
('Ansatt Tre', 'ansattre@alv.no', '2019-10-01', NULL, 3),
('Ansatt Fire', 'ansattfire@alv.no', '2019-11-01', '2020-05-08', 4),
('Ansatt Fem', 'ansattfem@alv.no', '2019-12-01', NULL, 5),
('Ansatt Seks', 'ansattseks@alv.no', '2019-08-01', '2021-02-01', 6),
('Ansatt Syv', 'ansattsyv@alv.no', '2019-09-01', NULL, 7),
('Ansatt Atte', 'ansattatte@alv.no', '2019-10-01', NULL, 8),
('Ansatt Ni', 'ansattni@alv.no', '2019-11-01', '2020-12-31', 9),
('Ansatt Ti', 'ansatti@alv.no', '2019-12-01', NULL, 10),
('Ahre Ketil Lillehagen', 'ahre-ketil.lillehagen@alvno.onmicrosoft.com', '2020-11-01', NULL, 11)


insert into [dbo].[Task]
values
('Testleder', '', 2, 0, 0, 0),
('Pålagt overtid', '', 2, 0, 0, 1),
('Funksjonell arkitekt', '', 2, 0, 0, 0),
('Teamleder', '', 6, 0, 0, 0),
('Prosjektstotte', '', 6, 0, 0, 0),
('Testleder', '', 3, 0, 0, 0),
('Testradgiver', '', 3, 0, 0, 0),
('Cost controller', '',	8, 0, 0, 0),
('Utvikler', '', 7, 0, 0, 0),
('Seniorutvikler', '', 1, 0, 0, 0),
('Juniorutvikler', '', 1, 0, 0, 0),
('Sikkerhetstester', '', 2, 0, 0, 0),
('Ferie', '', 8, 0, 0, 0),
('Interntid' ,'', 8, 0, 0, 0),
('Syk', '',	8, 0, 0, 0),
('Sykt barn', '', 8, 0, 0, 0),
('Youtube serie', '', 8, 0, 0, 0),
('Avspasering', '', 8, 0, 0, 0),
('AlvTimeUtvikling', '', 8, 0, 0, 0)

insert into [dbo].[TaskFavorites]
select id as UserId, 1 as TaskId
from [dbo].[User]

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

insert into [dbo].[Hours]
values
(11, 10,  '2022-03-28', 87, 2022, 1, 0, '2022-03-28 13:00:00'), /* Testleder: rate 1.5 */
(11, 11,  '2022-03-29', 88, 2022, 1, 0, '2022-03-29 13:00:00'), /* Testleder: rate 1.5 */
(11, 12,  '2022-03-30', 89, 2022, 12, 0, '2022-03-30 13:00:00'), /* Interntid: rate 1.0 */
(11, 13,  '2022-03-31', 90, 2022, 17, 0, '2022-03-31 13:00:00') /* AlvtimeUtvikling: rate 0.5 */

insert into [dbo].[Earnedovertime]
values
(11, 2.5, '2022-03-28', 1.5),
(11, 3.5, '2022-03-29', 1.5),
(11, 4.5, '2022-03-30', 1.0),
(11, 5.5, '2022-03-31', 0.5)

insert into [dbo].[HourRate]
values
('2018-12-12', 1000.00, 1),
('2018-12-12', 800.00, 2),
('2018-12-12', 700.00, 3),
('2018-12-12', 0.00, 4),
('2018-12-12', 500.00, 5),
('2018-12-12', 1200.00, 6),
('2018-12-12', 900.00, 12),
('2018-12-12', 0.00, 15)

insert into [dbo].[AccessTokens]
values
(11, 'TestToken', '2200-01-01', '5801gj90-jf39-5j30-fjk3-480fj39kl409'),
(11, 'Outdated', '2021-01-01', 'heiduder-jf39-5j30-fjk3-480fj39kl409')

insert into [dbo].[AssociatedTasks]
values
(1, 1, '2019-01-01', ''),
(2, 2, '2019-02-01', ''),
(3, 3, '2019-03-01', ''),
(4, 1, '2019-04-01', ''),
(5, 2, '2019-01-01', ''),
(6, 3, '2019-04-01', ''),
(7, 5, '2019-05-01', ''),
(8, 5, '2019-07-01', ''),
(9, 17, '2020-01-01', ''),
(10, 17, '2019-09-01', ''),
(11, 17, '2019-12-01', '')

insert into [dbo].[CompensationRate]
values
('2019-01-01', 1.5, 1),
('2019-02-01', 2.0, 2),
('2019-03-01', 1.5, 3),
('2019-04-01', 1.5, 4),
('2019-04-01', 1.5, 5),
('2019-04-01', 1.5, 6),
('2019-04-01', 1.5, 7),
('2019-04-01', 1.5, 8),
('2019-04-01', 1.5, 9),
('2019-04-01', 1.5, 10),
('2019-04-01', 1.5, 11),
('2019-04-01', 1.5, 12),
('2019-04-01', 1.0, 13),
('2019-04-01', 1.0, 14),
('2019-04-01', 1.0, 15),
('2019-04-01', 1.0, 16),
('2019-04-01', 1.0, 17),
('2019-04-01', 1.0, 18),
('2019-04-01', 1.0, 19)

insert into [dbo].[PaidOvertime]
values
('2022-03-31', 11, 5.5, 2.75, 0.5),
('2022-03-31', 11, 4.5, 4.5, 1.0),
('2022-03-31', 11, 2, 3, 1.5)