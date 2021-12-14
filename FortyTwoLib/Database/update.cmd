cd C:\dev\FortyTwo\FortyTwoLib\Database

sqlmetal /server:opincarhome /database:fortyTwo /dbml:FortyTwo.dbml /context:FortyTwoDataContext /functions /views /sprocs /namespace:FortyTwoLib.Database /user:fortytwodbaccess /password:titleist998
sqlmetal /server:opincarhome /database:fortyTwo /code:FortyTwo.designer.cs /context:FortyTwoDataContext /functions /views  /sprocs /namespace:FortyTwoLib.Database /user:fortytwodbaccess /password:titleist998 FortyTwo.dbml

