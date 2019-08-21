
insert into dbo.[User] (Id, EmailAddress)
select t.Id, t.EmailAddress
from (values
	('07b742cc-8c82-43b6-8615-de54635db929', 'your.email@gmail.com')
) as t (Id, EmailAddress)
where NOT EXISTS (SELECT Id FROM dbo.[User] WHERE [Id] = t.[Id])