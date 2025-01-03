Remove-Item -recurse **/*/obj
Remove-Item -recurse **/*/bin
dotnet msbuild -t:Restore
dotnet msbuild -t:Build -p:Configuration=Release src/ChatLinks
