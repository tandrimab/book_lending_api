# dotnet_practice
# it will create the gitignore files
dotnet new gitignore

# create a solution 
dotnet new sln -n BookLending

# create the domain. All class structures/behavious/methods will go inside this.
# the API layer, DB, redis and rest will not go here
dotnet new classlib -n BookLending.Domain -o src/BookLending.Domain

# playground for all the class
dotnet new console -n BookLending.Playground -o src/BookLending.Playground

# Add both to the solution, and let Playground use Domain
dotnet sln add src/BookLending.Domain src/BookLending.Playground
dotnet add src/BookLending.Playground reference src/BookLending.Domain
