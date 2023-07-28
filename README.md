# CLI-PasswordHasher
cli password hasher using aspnet identity that doesn't require a user object

if you have ever had a need to remove the pages for adding users on an aspnet identity site, this tool allows you to create the valid password hashes to insert into the database.

alternatively, you could use the same classes in whatever way you wanted. the passwordhasher classes were copied from microsofts repository and modified to remove the dependency on the user object so you can simply hash the password itself.
