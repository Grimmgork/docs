@echo off
set target=d:\document_vault
set count=0
set source=c:\users\eric\desktop\doc.gateway

:loop
echo:
echo number of documents: %count%
echo:
echo source directory:
echo ~ %source%
echo target directory:
echo ~ %target%
echo:
pause
echo:
set /p tags=Enter tags [t1,t2,t3]:

docs %source% -t %tags%

set /a count=count+1
echo ------------------------------------------
echo DONE!
echo:
goto loop