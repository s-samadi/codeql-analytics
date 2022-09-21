@ECHO OFF 

echo location of evaluator.json is %1
echo location of evaluator-summary.json is %2

set evaluator=%1
set evaluator-summary=%2 

for /F "delims=" %%i in (%evaluator%) do set evaldirname="%%~dpi" 
for /F "delims=" %%i in (%evaluator-summary%) do set evalsummdirname="%%~dpi"

REM Extract log header and log footer from evaluator log
cd %evaldirname%
jq ". | select(.type==\"LOG_HEADER\" or .type==\"LOG_FOOTER\")" %evaluator% > jq-evaluator.json
jq -c . jq-evaluator.json > jqlines-evaluator.json
REM Transform evaluator-summary json file
cd  %evalsummdirname%
jq -c . %evaluator-summary%  > jqlines-evaluator-summary.json
exit