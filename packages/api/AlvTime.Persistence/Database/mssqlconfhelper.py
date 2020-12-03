#!/usr/bin/python
# coding=utf-8
# Copyright (c) Microsoft. All rights reserved.

# Helper python script with utility methods

import sys
import os
import grp
import pwd
import getpass
import unicodedata
import subprocess
import platform
import gettext
import locale
import re
from ConfigParser import ConfigParser

#
# Static configuration values
#
sqlPathRoot = "/var/opt/mssql"
configurationFilePath = os.path.join(sqlPathRoot, "mssql.conf")
masterDatabaseFilePath = os.path.join(sqlPathRoot, "data", "master.mdf")
eulaConfigSection="EULA"
eulaConfigSetting="accepteula"
telemetryConfigSetting="customerfeedback"
telemetryLocalAuditCacheDirectorySetting="userrequestedlocalauditdirectory"
errorExitCode = 1
successExitCode = 0
directoryOfScript = os.path.dirname(os.path.realpath(__file__))
checkInstallScript = directoryOfScript + "/checkinstall.sh"
checkRunningInstanceScript = directoryOfScript + "/checkrunninginstance.sh"
invokeSqlservrScript = directoryOfScript + "/invokesqlservr.sh"
setCollationScript = directoryOfScript + "/set-collation.sh"
sudo = "sudo"
mssqlUser = "mssql"
saPasswordEnvVariable = "SA_PASSWORD"
mssqlSaPasswordEnvVariable = "MSSQL_SA_PASSWORD"
mssqlLcidEnvVariable = "MSSQL_LCID"
mssqlPidEnvVariable = "MSSQL_PID"
language = "language"
lcid = "lcid"
expressEdition = "express"
evaluationEdition = "evaluation"
developerEdition = "developer"
webEdition = "web"
standardEdition = "standard"
enterpriseEdition = "enterprise"
enterpriseCoreEdition = "enterprisecore"
supportedLcids = ['1033', '1031', '3082', '1036', '1040',
                  '1041', '1042', '1046', '1049', '2052', '1028']

#
# Colors to use when printing to standard out
#
class bcolors:
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    RED = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'

#
# Error codes for failed password validation
#
class passwordErrorCodes:
    SUCCESS        = 0
    TOO_SHORT      = 1
    TOO_LONG       = 2
    NOT_COMPLEX    = 3
    ENCODING_ERROR = 4
    DECODING_ERROR = 5
    CONTROL_CHARS  = 6

def printError(text):
    """printError

    Args:
        text(str): Text to print
    """

    _printTextInColor(text, bcolors.RED)

def printErrorUnsupportedSetting(unsupportedSetting):
    """Print error message and exit for unsupported settings

    Args:
        unsupportedSetting(str): Unsupported setting name
    """

    print(_("The setting '%s' is not supported.") % unsupportedSetting)
    exit(errorExitCode)

def checkColorSupported():
    """Check if color is supported

    Returns:
        True if color is supported, False otherwise
    """

    plat = sys.platform
    supported_platform = plat != 'Pocket PC' and (plat != 'win32' or 'ANSICON' in os.environ)
    is_a_tty = hasattr(sys.stdout, 'isatty') and sys.stdout.isatty()

    if not supported_platform or not is_a_tty:
        return False

    return True

def setSettingsFromEnvironment():
    """Set settings from environment
    """

    import mssqlsettingsmanager

    config = ConfigParser()

    for setting in mssqlsettingsmanager.supportedSettingsList:
        if setting.environment_variable in os.environ:
            # Make a local copy of environment variable and then remove
            # it from the environment. Otherwise it will be passed on
            # to SQL Server when running setup.
            #
            value = os.environ[setting.environment_variable]
            del os.environ[setting.environment_variable]

            readConfigFromFile(config, configurationFilePath)

            if len(value) > 0:
                if mssqlsettingsmanager.setSetting(config, setting, value, True):
                    writeConfigToFile(config, configurationFilePath)
                else:
                    return False
            else:
                if mssqlsettingsmanager.unsetSetting(config, setting):
                    writeConfigToFile(config, configurationFilePath)

    return True

def languageSelect(noprompt):
    """Select language

    Args:
        noprompt(boolean): True if --noprompt specified, false otherwise
    """

    lcidFromEnv = os.environ.get(mssqlLcidEnvVariable)

    if (lcidFromEnv != None):
        print _("Setting language using LCID from environment variable %s") % mssqlLcidEnvVariable
        writeLcidToConfFile(lcidFromEnv)
        return

    if(noprompt == False):
        language = locale.getdefaultlocale()[0]
        if(language == None or language == "" or language.lower() == "en_us"):
            # Nothing to do as en_US will be chosen by default by the engine
            return
        else:
            print ""
            print _("Choose the language for SQL Server:")
            print (u"(1) English")
            print (u"(2) Deutsch")
            print (u"(3) Español")
            print (u"(4) Français")
            print (u"(5) Italiano")
            print (u"(6) 日本語")
            print (u"(7) 한국어")
            print (u"(8) Português")
            print (u"(9) Русский")
            print (u"(10) 中文 – 简体")
            print (u"(11) 中文 （繁体）")

            languageOption = raw_input(_("Enter Option 1-11: "))

            optionToLcid = { '1': '1033', #en-US
                     '2': '1031', #de-DE
                     '3': '3082', #es-ES
                     '4': '1036', #fr-FR
                     '5': '1040', #it-IT
                     '6': '1041', #ja-JP
                     '7': '1042', #ko-KR
                     '8': '1046', #pt-BR
                     '9': '1049', #ru-RU
                     '10': '2052', #zh-CN
                     '11': '1028'} #zh-TW

            if (languageOption in optionToLcid.keys()):
                writeLcidToConfFile(optionToLcid[languageOption])
            else:
                print _("Invalid Option. Exiting.")
                exit(errorExitCode)


def isValidLcid(lcidValue):
    """Check if a LCID value is valid.

    Args:
        lcidValue(int): LCID value
    """

    return lcidValue in supportedLcids

def writeLcidToConfFile(lcidValue):
    """Write LCID to configuration file

    Args:
        lcidValue(int): LCID value
    """

    if (isValidLcid(lcidValue) == False):
        print _("LCID %s is not supported.") % lcidValue
        exit(errorExitCode)

    config = ConfigParser(allow_no_value=True)
    readConfigFromFile(config, configurationFilePath)

    if (config.has_section(language) == False):
        config.add_section(language)

    config.set(language, lcid, lcidValue)
    writeConfigToFile(config, configurationFilePath)

def printEngineWelcomeMessage():
    """Print engine welcome message
    """

    print ("")
    print ("+--------------------------------------------------------------+")
    print (_("Please run 'sudo /opt/mssql/bin/mssql-conf setup'"))
    print (_("to complete the setup of Microsoft SQL Server"))
    print ("+--------------------------------------------------------------+")
    print ("")
    return successExitCode

def printAgentWelcomeMessage():
    """Print agent welcome message
    """

    print ("")
    print ("+--------------------------------------------------------------------------------+")
    print (_("Please restart mssql-server to enable Microsoft SQL Server Agent."))
    print ("+--------------------------------------------------------------------------------+")
    print ("")

def printFTSWelcomeMessage():
    """Print full-text welcome message
    """

    print ("")
    print ("+-------------------------------------------------------------------------------------+")
    print (_("Please restart mssql-server to enable Microsoft SQL Server Full Text Search."))
    print ("+-------------------------------------------------------------------------------------+")
    print ("")

def getFwlinkWithLocale(linkId):
    """Gets the correct Url for the fwlink based on the users locale
   
    Args:
        linkId(string): The fwlink ID

    Returns:
        The string with the complete url
    """

    baseUrl = "https://go.microsoft.com/fwlink/?LinkId=" + linkId
    localeCode = locale.getlocale()[0]
    localeToClcid = {'en_US': '0x409',  # en-US
                     'de_DE': '0x407',  # de-DE
                     'es_ES': '0x40a',  # es-ES
                     'fr_FR': '0x40c',  # fr-FR
                     'it_IT': '0x410',  # it-IT
                     'ja_JP': '0x411',  # ja-JP
                     'ko_KR': '0x412',  # ko-KR
                     'pt_BR': '0x416',  # pt-BR
                     'ru_RU': '0x419',  # ru-RU
                     'zh_CN': '0x804',  # zh-CN
                     'zh_TW': '0x404'}  # zh-TW

    if localeCode in localeToClcid:
        return baseUrl + "&clcid=" + localeToClcid[localeCode]
    else:
        return baseUrl


def checkEulaAgreement(eulaAccepted, configurationFilePath, ignoreMasterDatabase=False, isEvaluationEdition = False):
    """Check if the EULA agreement has been accepted.

    Args:
        eulaAccepted(boolean): User has indicated their acceptance via command-line
                               or environment variable.
        configurationFilePath(str): Configuration file path
        ignoreMasterDatabase(boolean): Ignore presence of master database
        isEvaluationEdition(boolean): True if edition selected is evaluation, false otherwise

    Returns:
        True if accepted, False otherwise
    """

    print(_("The license terms for this product can be found in"))
    print("/usr/share/doc/mssql-server " + _("or downloaded from:"))
    if isEvaluationEdition:
        print(getFwlinkWithLocale("855864"))
    else:
        print(getFwlinkWithLocale("855862"))
    print("")
    print(_("The privacy statement can be viewed at:"))
    print(getFwlinkWithLocale("853010"))
    print("") 

    if os.path.exists(masterDatabaseFilePath) and not ignoreMasterDatabase:
        return True

    config = ConfigParser(allow_no_value=True)
    readConfigFromFile(config, configurationFilePath)

    if (config.has_section(eulaConfigSection) == False or \
        config.get(eulaConfigSection, eulaConfigSetting) is None):
        if not eulaAccepted:
            agreement = raw_input(_("Do you accept the license terms?") + " [Yes/No]:")
            print("")

            if (agreement.strip().lower() == "yes" or agreement.strip().lower() == "y"):
                eulaAccepted = True
            else:
                return False

        if eulaAccepted:
            config.add_section(eulaConfigSection)
            config.set(eulaConfigSection, eulaConfigSetting, "Y")
            writeConfigToFile(config, configurationFilePath)
            return True

    return True

def checkSudo():
    """Check if we're running as root

    Returns:
        True if running as root, False otherwise
    """

    if (os.geteuid() == 0):
        return True

    return False

def checkSudoOrMssql():
    """Check if we're running as root or the user is in the mssql group.

    Returns:
        True if running as root or in mssql group, False otherwise
    """

    if(checkSudo() == True):
        return True

    user = getpass.getuser()
    groups = [g.gr_name for g in grp.getgrall() if user in g.gr_mem]
    gid = pwd.getpwnam(user).pw_gid
    groups.append(grp.getgrgid(gid).gr_name)

    if(('mssql' in groups) and (user == mssqlUser)):
        return True

    return False

def printValidationErrorMessage(setting, errorMessage):
    """Print a validation error message.

    Args:
        setting(str): Setting name
        errorMessage(str): Error message
    """

    printError(_("Validation error on setting '%s.%s'") % (setting.section, setting.name))
    printError(errorMessage)

def printPasswordErrorMessage(errorCode):
    """Print an error message if password can't be validated.

     Args:
         errorCode(int): Error code
    """

    if errorCode == passwordErrorCodes.TOO_SHORT:
        printError((_("The specified password does not meet SQL Server password policy requirements because it is "
                "too short. The password must be at least 8 characters")))
    elif errorCode == passwordErrorCodes.TOO_LONG:
        printError((_("The specified password does not meet SQL Server password policy requirements because it is "
                "too long. The password cannot exceed 128 characters")))
    elif errorCode == passwordErrorCodes.NOT_COMPLEX:
        printError((_("The specified password does not meet SQL Server password policy requirements because it "
                      "is not complex enough. The password must be at least 8 characters long and contain characters "
                      "from three of the following four sets: uppercase letters, lowercase letters, numbers, "
                      "and symbols.")))
    elif errorCode == passwordErrorCodes.ENCODING_ERROR:
        printError(_("The specified password contains a character that cannot be encoded to UTF-8. "
                         "Try using a password with only ASCII characters."))
    elif errorCode == passwordErrorCodes.DECODING_ERROR:
        printError(_("The specified password contains a character that cannot be decoded. "
                         "Try using a password with only ASCII characters."))
    elif errorCode == passwordErrorCodes.CONTROL_CHARS:
        printError((_("The specified password contains an invalid character. Valid characters "
                               "include uppercase letters, lowercase letters, numbers, symbols, "
                               "punctuation marks, and unicode characters that are categorized as alphabetic "
                               "but are not uppercase or lowercase.")))

def makeDirectoryIfNotExists(directoryPath):
    """Make a directory if it does not exist

    Args:
        directoryPath(str): Directory path
    """

    try:
        if os.path.exists(directoryPath):
            return
        if not os.path.exists(os.path.dirname(directoryPath)):
            makeDirectoryIfNotExists(os.path.dirname(directoryPath))
        os.makedirs(directoryPath)
    except IOError, err:
        if err.errno == 13:
            printError(_("Permission denied to mkdir '%s'.") % (directoryPath))
            exit(errorExitCode)
        else:
            printError(err)
            exit(errorExitCode)

def writeConfigToFile(config, configurationFilePath):
    """Write configuration to a file

    Args:
        config(object): Config parser object
        configurationFilePath(str): Configuration file path
    """

    makeDirectoryIfNotExists(os.path.dirname(configurationFilePath))

    try:
        with open(configurationFilePath, 'w') as configFile:
            config.write(configFile)
    except IOError, err:
        if err.errno == 13:
            printError(_("Permission denied to modify SQL Server configuration."))
        else:
            printError(err)
            exit(errorExitCode)

def readConfigFromFile(config, configurationFilePath):
    """"Read configuration from a file

    Args:
        config(object): Config parser object
        configurationFilePath(str): Configuration file path
    """

    if (os.path.exists(configurationFilePath) == True):
        try:
            config.read(configurationFilePath)
        except:
            printError(_("There was a parsing error in the configuration file."))
            exit(errorExitCode)

def listSupportedSettings(supportedSettingsList):
    """List supported settings
    """

    maxLength = 0

    for setting in supportedSettingsList:
        settingLength = len("%s.%s" % (setting.section, setting.name))
        if settingLength > maxLength:
            maxLength = settingLength

    def getSettingSortKey(item):
        return "%s.%s" % (item.section, item.name)

    formatString = "%-" + str(maxLength) + "s %s"
    for setting in sorted(supportedSettingsList, key=getSettingSortKey):
        if setting.hidden == False:
            print(formatString % ("%s.%s" % (setting.section, setting.name), setting.description))

    exit(successExitCode)

def printRestartRequiredMessage():
    """Print a message telling the user that SQL Server needs to be restarted.
    """

    print(_("SQL Server needs to be restarted in order to apply this setting. Please run"))
    print(_("'systemctl restart mssql-server.service'."))

def printStartSqlServerMessage():
    """Print a message telling the user to start SQL Server.
    """

    print(_("Please run 'sudo systemctl start mssql-server' to start SQL Server."))

def validateCollation(collation):
    """Validate collation

    Args:
        collation(str): Collation name
    """

    directoryOfScript = os.path.dirname(os.path.realpath(__file__))
    with open(directoryOfScript + '/collations.txt') as f:
        supportedCollationsList = [line.strip() for line in f.readlines()]
    if collation.lower() in (collation.lower() for collation in supportedCollationsList):
        return True
    else:
        printError(_("'%s' is not a supported SQL Server collation.") % collation)
        return False

def validatePasswordAndPrintIfError(password):
    """Validate a password and print an error if it's not formed correctly.

    Args:
         password(str): Password

    Returns:
         True if valid, False otherwise
    """

    passwordValidationResult = isValidPassword(password)

    if passwordValidationResult != passwordErrorCodes.SUCCESS:
        printPasswordErrorMessage(passwordValidationResult)
        return False
    else:
        return True

def getSystemAdministratorPassword(noprompt):
    """Get the system administrator password from the user via environment
    variable or interactively.

    Returns:
        UTF-8 encoded system administrator password
    """

    if checkRunningInstance():
        return None

    passwordFromEnvironmentVariable = os.environ.get(mssqlSaPasswordEnvVariable)

    # If MSSQL_SA_PASSWORD is not set, fall back to SA_PASSWORD
    #
    if passwordFromEnvironmentVariable is None:
        passwordFromEnvironmentVariable = os.environ.get(saPasswordEnvVariable)

    if (passwordFromEnvironmentVariable != None):
        if(validatePasswordAndPrintIfError(passwordFromEnvironmentVariable) == False):
            return errorExitCode
        return encodeSystemAdministratorPassword(passwordFromEnvironmentVariable)

    if not noprompt or noprompt == False:
        return encodeSystemAdministratorPassword(getSystemAdministratorPasswordInteractive())

    print(_("The MSSQL_SA_PASSWORD environment variable must be set in order to change the"))
    print(_("system administrator password."))

    return None

def getSystemAdministratorPasswordInteractive():
    """Get the system administrator password from the user interactively.

    Returns:
        System administrator password
    """

    while True:
        saPassword = getpass.getpass(_("Enter the SQL Server system administrator password: "))

        if validatePasswordAndPrintIfError(saPassword) == False:
            continue

        saPasswordConfirm = getpass.getpass(_("Confirm the SQL Server system administrator password: "))

        if (saPassword != saPasswordConfirm):
            printError(_("The passwords do not match. Please try again."))
            continue

        break

    return saPassword

def encodeSystemAdministratorPassword(password):
    """Encode system administrator password in UTF-8 format

    Args:
        noprompt(bool): Don't prompt user if True

    Returns:
        UTF-8 encoded system administrator password
    """

    # Warn the user if the system encoding cannot be detected.
    #
    if sys.stdin.encoding is None:
        print((_("Input encoding cannot be detected. SQL Server will attempt to "
                            "decode input as UTF-8")))
        encoding = 'utf-8'
    else:
        encoding = sys.stdin.encoding

    encodedPassword = password.decode(encoding).encode('utf-8')

    return encodedPassword

def validatePid(pid):
    """Validate a product key

    Args:
        pid(str): Product key

    Returns:
        Product key if valid, otherwise None
    """

    if not (
        pid.lower() == expressEdition or
        pid.lower() == evaluationEdition or
        pid.lower() == developerEdition or
        pid.lower() == webEdition or
        pid.lower() == standardEdition or
        pid.lower() == enterpriseEdition or
        pid.lower() == enterpriseCoreEdition or
        re.match("^([A-Za-z0-9]){5}-([A-Za-z0-9]){5}\-([A-Za-z0-9]){5}\-([A-Za-z0-9]){5}\-([A-Za-z0-9]){5}$", pid)
    ):
        printError(_("Invalid PID specified: %s.") % (pid))
        print("")
        return None

    return pid

def getPidFromEditionSelected(edition):
    """Gets the correct pid to pass to the engine 
    
    Args:
        edition(string): Edition option 1-8

    Returns:
        Pid as expected by the engine

    """

    if edition == "1":
        return evaluationEdition
    elif edition == "2":
        return developerEdition
    elif edition == "3":
        return expressEdition
    elif edition == "4":
        return webEdition
    elif edition == "5":
        return standardEdition
    elif edition == "6":
        return enterpriseEdition
    elif edition == "7":
        return enterpriseCoreEdition
    elif edition == "8":
        while(True):
            productKey = raw_input(_("Enter the 25-character product key: "))
            print("")
            if validatePid(productKey):
                break
        return productKey

    else:
        print(_("Invalid option %s.") % edition)
        exit(errorExitCode)

def getPid(noprompt=False):
    """Get product key from user

    Args:
        noprompt(bool): Don't prompt user if True

    Returns:
        Product key
    """

    pidFromEnv = os.environ.get(mssqlPidEnvVariable)

    if (pidFromEnv != None):
        return validatePid(pidFromEnv)

    # If running with --noprompt and MSSQL_PID not set return developer edition
    #
    if (noprompt):
        return developerEdition

    print(_("Choose an edition of SQL Server:"))
    print("  1) Evaluation " + _("(free, no production use rights, 180-day limit)"))
    print("  2) Developer " + _("(free, no production use rights)"))
    print("  3) Express " + _("(free)"))
    print("  4) Web " + _("(PAID)"))
    print("  5) Standard " + _("(PAID)"))
    print("  6) Enterprise " + _("(PAID)"))
    print("  7) Enterprise Core " + _("(PAID)"))
    print("  8) " + _("I bought a license through a retail sales channel and have a product key to enter."))
    print("")
    print(_("Details about editions can be found at"))
    print(getFwlinkWithLocale("852748"))
    print("")
    print(_("Use of PAID editions of this software requires separate licensing through a"))
    print(_("Microsoft Volume Licensing program."))
    print(_("By choosing a PAID edition, you are verifying that you have the appropriate"))
    print(_("number of licenses in place to install and run this software."))
    print("")
    edition = raw_input(_("Enter your edition") + "(1-8): " )

    pid = getPidFromEditionSelected(edition)
    return validatePid(pid)

def configureSqlservrWithArguments(*args, **kwargs):
    """Configure SQL Server with arguments

    Args:
        args(str): Parameters to SQL Server
        kwargs(dict): Environment variables

    Returns:
        SQL Server exit code
    """

    args = [invokeSqlservrScript] + list(args)
    env = dict(os.environ)
    env.update(kwargs)
    env = {str(k): str(v) for k, v in env.items()}
    print(_("Configuring SQL Server..."))
    return subprocess.call(args, env=env)

def runScript(pathToScript, runAsRoot=False):
    runAsRoot = False
    """Runs a script (optionally as root)

    Args:
        pathToScript(str): Path to script to run
        runAsRoot(boolean): Run script as root or not

    Returns:
        Script exit code
    """

    if (runAsRoot):
        if(checkSudo() == False):
            printError(_("Elevated privileges required for this action. Please run in 'sudo' mode."))
            return (errorExitCode)
        return subprocess.call([sudo, "-EH", pathToScript])
    else:
        return subprocess.call([pathToScript])

def checkInstall():
    """Checks installation of SQL Server

    Returns:
        True if there are no problems, False otherwise
    """

    return runScript(checkInstallScript, True) == 0

def checkRunningInstance():
    """Check if an instance of SQL Server is running

    Returns:
        True if there is an instance running, False otherwise
    """

    ret = runScript(checkRunningInstanceScript)

    if (ret == 0):
        print (_("An instance of SQL Server is running. Please stop the SQL Server service"))
        print (_("using the following command"))
        print ("")
        print ("    sudo systemctl stop mssql-server")
        return True

    return False

def setupSqlServer(eulaAccepted, noprompt=False):
    """Setup and initialize SQL Server

    Args:
        eulaAccepted (boolean): Whether Eula was accepted on command line or via env variable
        noprompt (boolean): Don't prompt if True
    """

    # Make sure installation basics are OK
    #
    if not checkInstall():
        exit(errorExitCode)

    # Check if SQL Server is already running
    #
    if checkRunningInstance():
        exit(errorExitCode)

    # Get product key
    #
    pid = getPid(noprompt)

    if(noprompt == False and pid is None):
        exit(errorExitCode)

    # Check for EULA acceptance and show EULA based on edition selected
    if not checkEulaAgreement(eulaAccepted, configurationFilePath, isEvaluationEdition = (pid == evaluationEdition)):
        printError(_("EULA not accepted. Exiting."))
        exit(errorExitCode)

    # Select language and write LCID to configuration
    #
    languageSelect(noprompt)

    # Set settings from environment
    #
    if (not setSettingsFromEnvironment()):
        exit(errorExitCode)

    # Get system administrator password
    #
    encodedPassword = getSystemAdministratorPassword(noprompt)
    if (encodedPassword == errorExitCode or encodedPassword == None):
        exit(errorExitCode)

    if (pid == None):
        ret = configureSqlservrWithArguments("--setup --reset-sa-password", MSSQL_SA_PASSWORD=encodedPassword)
    else:
        ret = configureSqlservrWithArguments("--setup --reset-sa-password", MSSQL_SA_PASSWORD=encodedPassword, MSSQL_PID=pid)

    if (ret == errorExitCode):
        print(_("Initial setup of Microsoft SQL Server failed. Please consult the ERRORLOG"))
        print(_("in /var/opt/mssql/log for more information."))
        exit(ret)

    # Start the SQL Server service
    #
    ret = subprocess.call(["sudo", "-u", "mssql", "/opt/mssql/bin/sqlservr", "&"])
    if (ret != 0):
        print (_("Attempting to start the Microsoft SQL Server service failed."))
        exit(ret)

    # Enable SQL Server to run at startup
    #
    #ret = subprocess.call(["sudo", "systemctl", "enable", "mssql-server"])
    #if (ret != 0):
    #    print (_("Attempting to enable the Microsoft SQL Server to start at boot failed."))
    #    exit(ret)

    print(_("Setup has completed successfully. SQL Server is now starting."))

def isValidPassword(password):
    """Determines if a given password matches SQL Server policy requirements

    Args:
        password (str): a password encoded as described by sys.stdin.encoding

    Returns:
        Integer error code. 0 for success, [1-6] otherwise. Error codes are defined
        in the passwordErrorCodes class.

    Notes:
        This function cannot print anything to stdout or it will interfere with
        the validate-password flag in mssql-conf.py
    """

    # Decode the password as whatever the input encoding is.
    # If the environmental variable is not set, then we have no way of knowing the
    # encoding of the input text. The user will have already received a warning message.
    #
    inputEncoding = sys.stdin.encoding
    if inputEncoding is None:
        inputEncoding = 'utf-8'

    # Characters that Windows treats as "special characters" for its password validation.
    #
    SYMBOL_CHARACTERS = "(`~!@#$%^&*_-+=|\\{}[]:;\"'<>,.?)/"

    # Known bug:
    # This function will return the incorrect character count for multi-byte encodings
    # For example, a smiley face emoji, U+1F601 will cause this function to allow shorter
    # passwords. However, this matches the behavior of Windows password policy validation,
    # and will thus cause no compatability issues or information loss with SQL Server.
    #
    if len(password) < 8:
        return passwordErrorCodes.TOO_SHORT
    if len(password) > 128:
        return passwordErrorCodes.TOO_LONG

    # Exit gracefully if we get a character that cannot be decoded.
    try:
        unicodePassword = password.decode(inputEncoding)
    except (UnicodeEncodeError, UnicodeDecodeError):
        return passwordErrorCodes.DECODING_ERROR

    # Exit if we cannot convert the password to UTF-8 for the PAL
    #
    try:
        testOutput = unicodePassword.encode('utf-8')
    except (UnicodeEncodeError, UnicodeDecodeError):
        return passwordErrorCodes.ENCODING_ERROR

    # Count the number of occurences of each type of character
    #
    containsUpper = 0
    containsLower = 0
    containsNumeric = 0
    containsSymbol = 0

    for unicodeChar in unicodePassword:
        # Gets the general category assigned to a given unicode character
        # If it does not fall into any of these categories (For example, Sm, which includes U+221A)
        # then it is treated as a character for length, but does not count toward any category.
        #
        # This does not perfectly match the way that Windows evaluates characters, but is a
        # proper subset of characters accepted by Windows
        #
        # If a character is a "control character" like escape, then we print an error.
        #
        char_category = unicodedata.category(unicodeChar)
        if unicodeChar in SYMBOL_CHARACTERS:
            containsSymbol = 1
        elif char_category == 'Ll':
            containsLower = 1
        elif char_category == 'Lu':
            containsUpper = 1
        elif char_category == 'Nd':
            containsNumeric = 1
        elif char_category == 'Cc':
            return passwordErrorCodes.CONTROL_CHARS

    if (containsUpper + containsLower + containsNumeric + containsSymbol) >= 3:
        return passwordErrorCodes.SUCCESS

    else:
        return passwordErrorCodes.NOT_COMPLEX

def _printTextInColor(text, bcolor):
    """_printTextInColor

    Args:
        text(str): Text to print
        bcolor(int): Color to use
    """

    if (checkColorSupported()):
        print(bcolor + text + bcolors.ENDC)
    else:
        print(text)

def initialize():
    """Initialize mssqlconfhelper
    """

    try:
        defaultMoFilePath = os.path.dirname(os.path.realpath(__file__)) + "/loc/mo/mssql-conf-en_US.mo"
        locale.setlocale(locale.LC_ALL, '')
        localeCode = locale.getlocale()[0]

        if (localeCode == None):
            moFilePath = defaultMoFilePath
        else:
            moFilePath = os.path.dirname(os.path.realpath(__file__)) + "/loc/mo/mssql-conf-" + localeCode + ".mo"
            if (os.path.isfile(moFilePath) == False):
                print ("Locale %s not supported. Using en_US." % localeCode)
                moFilePath = defaultMoFilePath
    except:
        print ("Error in localization. Using en_US.")
        moFilePath = defaultMoFilePath

    locstrings = gettext.GNUTranslations(open(moFilePath, "rb"))
    locstrings.install()

def getErrorLogFile(configFilePath = configurationFilePath):
    """Get error log file
    """
    config = ConfigParser()
    readConfigFromFile(config, configFilePath)

    try:
        errorlog = config.get("filelocation", "errorlogfile")
    except:
        errorlog = '/var/opt/mssql/log'

    return errorlog