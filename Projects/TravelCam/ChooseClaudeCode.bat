@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

title Claude Code Multi-Provider Selector (April 2026)
echo.
echo ================================================
echo      Claude Code Multi-Provider Selector
echo ================================================
echo.

:MAIN_CHOICE
echo 1. Claude (Anthropic Cloud - sonnet)
echo 2. Ollama (Remote: 192.168.0.104)
echo 3. Oobabooga (Remote: 192.168.0.104)
echo 4. OpenRouter (Cloud Gateway)
echo.
choice /c 1234 /n /m "Select Provider (1-4): "

:: --- RESET ALL ENVIRONMENT VARIABLES ---
set ANTHROPIC_BASE_URL=
set ANTHROPIC_AUTH_TOKEN=
set ANTHROPIC_API_KEY=
set ANTHROPIC_MODEL=
set ANTHROPIC_DEFAULT_OPUS_MODEL=
set ANTHROPIC_DEFAULT_SONNET_MODEL=
set ANTHROPIC_DEFAULT_HAIKU_MODEL=
set CLAUDE_CODE_SKIP_AUTH=

if errorlevel 4 goto OPENROUTER
if errorlevel 3 goto OOBA_REMOTE
if errorlevel 2 goto OLLAMA_REMOTE
if errorlevel 1 goto CLAUDE

:: --- 1. ANTHROPIC CLOUD ---
:CLAUDE
set MODEL_NAME=sonnet
:: Reset Skip Auth so you can use your normal subscription login
set CLAUDE_CODE_SKIP_AUTH=
goto START_EXECUTION

:: --- 2. OLLAMA ---
:OLLAMA_REMOTE
set MODEL_NAME=qwen3.5:14b
set ANTHROPIC_BASE_URL=http://192.168.0.104:11434
set ANTHROPIC_AUTH_TOKEN=ollama
goto START_EXECUTION

:: --- 3. OOBABOOGA ---
:OOBA_REMOTE
set MODEL_NAME=Qwen3-14B-DeepSeek-v3.2-Speciale-Distill.q5_k_m.gguf
set ANTHROPIC_BASE_URL=http://192.168.0.104:5000/v1
set ANTHROPIC_AUTH_TOKEN=oobabooga
goto START_EXECUTION

:: --- 4. OPENROUTER ---
:OPENROUTER
echo.
set /p USER_KEY="Paste OpenRouter API Key (sk-or-...): "
echo.

echo 1. Step-3.5-Flash (Free - fast & cheap)
echo 2. Minimax-M2.5 (stronger)
echo.
choice /c 12 /n /m "Choice: "

if errorlevel 2 (
    set TARGET_MODEL=minimax/minimax-m2.5
) else (
    set TARGET_MODEL=stepfun/step-3.5-flash:free
)

:: === CORRECT OPENROUTER CONFIG (2026) ===
set ANTHROPIC_BASE_URL=https://openrouter.ai/api
set ANTHROPIC_AUTH_TOKEN=%USER_KEY%
set ANTHROPIC_API_KEY=               :: MUST be empty!

:: Map all three Claude roles to the same model (prevents "model not found")
set ANTHROPIC_DEFAULT_OPUS_MODEL=%TARGET_MODEL%
set ANTHROPIC_DEFAULT_SONNET_MODEL=%TARGET_MODEL%
set ANTHROPIC_DEFAULT_HAIKU_MODEL=%TARGET_MODEL%

:: Optional: also set the main model variable
set ANTHROPIC_MODEL=%TARGET_MODEL%
set MODEL_NAME=%TARGET_MODEL%

goto START_EXECUTION

:: --- FINAL EXECUTION ---
:START_EXECUTION
echo.
echo ------------------------------------------------
echo CONFIGURATION SUMMARY
echo ------------------------------------------------
echo Target Model:     %MODEL_NAME%
if defined ANTHROPIC_BASE_URL echo API Endpoint:    %ANTHROPIC_BASE_URL%
if defined ANTHROPIC_AUTH_TOKEN echo Auth Mode:       AUTH_TOKEN (OpenRouter/Ollama/Ooba)
if defined ANTHROPIC_API_KEY echo ANTHROPIC_API_KEY is set (should be empty for proxies)
echo ------------------------------------------------
echo.

:: Launch Claude Code
if defined ANTHROPIC_BASE_URL (
    echo Launching with custom provider...
    claude
) else (
    echo Launching native Anthropic...
    claude --model %MODEL_NAME%
)

echo.
echo Session ended.
pause
goto END

:END
endlocal