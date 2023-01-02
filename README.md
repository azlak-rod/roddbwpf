# roddbwpf

My take on the awesome Realms of Despair item db shared with me by Threndor, and is originally from the Druidic Cartographers. I've made no changes to the itemdb file and included it in this repo
so that everything you need is present to run.

## Searching

It is possible to search by stat (case insensitively) using >. For example, `str>0` or `STR>0` will both work.
You can mix and match with regular keywords, e.g. `dragon str>0` will give you items with Dragon in the name and
atleast 1 strength.

## Pre-built binaries

There is a github action on push/PR that generates artifacts. Navigate to the Actions tab and find the latest build with a green checkmark then look
at the bottom under 'Artifacts'. There you will find a ~1.5MB zip file that has everything you will need.

## Building from source

This is built using .NET 7 (you will need to install the .NET 7 SDK).
Then you can just `dotnet build` and it should work. (You may need to `dotnet restore` first.)

## Future development
* Integration with mud clients for item collection
* Adding area info to db schema