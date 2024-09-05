import 'package:alv_time_app/core/configuration/types.dart';

final configurationDevelopment = AppConfiguration(
    authenticationConfiguration: AuthenticationConfiguration(
        authority:
            'https://login.microsoftonline.com/76749190-4427-4b08-a3e4-161767dd1b73/v2.0',
        clientId: '5ad0b180-b2ad-43e8-bc4c-fba59a4b5108',
        //redirectUrl: "$appPackageName://oauthredirect",
        redirectUrl: "login.microsoftonline.com/common/oauth2/nativeclient",
        customRedirectScheme: "https",
        mobileCustomScheme: "msauth",
        mobileRedirectUri:
            "alv.time.app/%2Frn0m6TJIR79gIT%2BHb%2FZVR1V3%2Bc%3D",
        scopes: [
          "openid",
          "profile",
        ]),
    backendUri: Uri(host: 'alvtime.no'));

final configurationProduction = AppConfiguration(
    authenticationConfiguration: AuthenticationConfiguration(
        authority: '',
        clientId: '',
        redirectUrl: '',
        scopes: [],
        customRedirectScheme: '',
        mobileRedirectUri: '',
        mobileCustomScheme: ''),
    backendUri: Uri(host: 'alvtime.no'));
