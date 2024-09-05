import 'dart:convert';
import 'dart:io';
import 'package:oauth2_client/oauth2_client.dart';
import 'package:http/http.dart';

import 'package:alv_time_app/core/dependency_injection.dart';
import 'package:oauth2_client/oauth2_helper.dart';

Future<OAuth2Helper> createOAuth2Client() async {
  final configuration = getConfiguration();

  final discoveryClient = OAuthDiscoveryClient(
      authority: configuration.authenticationConfiguration.authority);
  final discoveryDocument = await discoveryClient.getDiscoveryDocument();

  String customUriScheme = "";
  String redirectUri = "";

  if (Platform.isAndroid || Platform.isIOS) {
    customUriScheme =
        configuration.authenticationConfiguration.mobileCustomScheme;
    redirectUri = configuration.authenticationConfiguration.mobileRedirectUri;
  } else if (Platform.isLinux || Platform.isWindows || Platform.isMacOS) {
    customUriScheme =
        configuration.authenticationConfiguration.customRedirectScheme;
    redirectUri = configuration.authenticationConfiguration.redirectUrl;
  } else {
    // TODO We are in a browser
  }

  final oauthClient = MicrosoftOAuth2Client(
      authorizeUrl: discoveryDocument!.authorizationEndpoint ?? "",
      tokenUrl: discoveryDocument.tokenEndpoint ?? "",
      redirectUri: "$customUriScheme://$redirectUri",
      customUriScheme: customUriScheme);

  final OAuth2Helper auth2helper = OAuth2Helper(oauthClient,
      clientId: configuration.authenticationConfiguration.clientId,
      grantType: OAuth2Helper.authorizationCode,
      enablePKCE: true,
      scopes: configuration.authenticationConfiguration.scopes);

  return auth2helper;
}

class MicrosoftOAuth2Client extends OAuth2Client {
  MicrosoftOAuth2Client(
      {required super.authorizeUrl,
      required super.tokenUrl,
      required super.redirectUri,
      required super.customUriScheme});
}

class OAuthDiscoveryClient {
  final String authority;

  OAuthDiscoveryClient({required this.authority});

  Future<DiscoveryDocument?> getDiscoveryDocument() async {
    final client = Client();
    var authorityUri = Uri.parse(authority);
    var wellKnownUri = Uri(
        scheme: authorityUri.scheme,
        fragment: authorityUri.fragment,
        host: authorityUri.host,
        port: authorityUri.port,
        pathSegments: [
          ...authorityUri.pathSegments,
          '.well-known',
          'openid-configuration'
        ]);

    try {
      final response = await client.get(wellKnownUri);
      final responseMap = json.decode(response.body) as Map<String, dynamic>;
      return DiscoveryDocument.fromJson(responseMap);
    } catch (e) {
      throw "Unable to do OIDC discovery. Have you configured a correct authority?";
    } finally {
      client.close();
    }
  }
}

class DiscoveryDocument {
  String? tokenEndpoint;
  List<String>? tokenEndpointAuthMethodsSupported;
  String? jwksUri;
  List<String>? responseModesSupported;
  List<String>? subjectTypesSupported;
  List<String>? idTokenSigningAlgValuesSupported;
  List<String>? responseTypesSupported;
  List<String>? scopesSupported;
  String? issuer;
  bool? requestUriParameterSupported;
  String? userinfoEndpoint;
  String? authorizationEndpoint;
  String? deviceAuthorizationEndpoint;
  bool? httpLogoutSupported;
  bool? frontchannelLogoutSupported;
  String? endSessionEndpoint;
  List<String>? claimsSupported;
  String? kerberosEndpoint;
  String? tenantRegionScope;
  String? cloudInstanceName;
  String? cloudGraphHostName;
  String? msgraphHost;
  String? rbacUrl;

  DiscoveryDocument(
      {this.tokenEndpoint,
      this.tokenEndpointAuthMethodsSupported,
      this.jwksUri,
      this.responseModesSupported,
      this.subjectTypesSupported,
      this.idTokenSigningAlgValuesSupported,
      this.responseTypesSupported,
      this.scopesSupported,
      this.issuer,
      this.requestUriParameterSupported,
      this.userinfoEndpoint,
      this.authorizationEndpoint,
      this.deviceAuthorizationEndpoint,
      this.httpLogoutSupported,
      this.frontchannelLogoutSupported,
      this.endSessionEndpoint,
      this.claimsSupported,
      this.kerberosEndpoint,
      this.tenantRegionScope,
      this.cloudInstanceName,
      this.cloudGraphHostName,
      this.msgraphHost,
      this.rbacUrl});

  DiscoveryDocument.fromJson(Map<String, dynamic> json) {
    tokenEndpoint = json['token_endpoint'];
    tokenEndpointAuthMethodsSupported =
        json['token_endpoint_auth_methods_supported'].cast<String>();
    jwksUri = json['jwks_uri'];
    responseModesSupported = json['response_modes_supported'].cast<String>();
    subjectTypesSupported = json['subject_types_supported'].cast<String>();
    idTokenSigningAlgValuesSupported =
        json['id_token_signing_alg_values_supported'].cast<String>();
    responseTypesSupported = json['response_types_supported'].cast<String>();
    scopesSupported = json['scopes_supported'].cast<String>();
    issuer = json['issuer'];
    requestUriParameterSupported = json['request_uri_parameter_supported'];
    userinfoEndpoint = json['userinfo_endpoint'];
    authorizationEndpoint = json['authorization_endpoint'];
    deviceAuthorizationEndpoint = json['device_authorization_endpoint'];
    httpLogoutSupported = json['http_logout_supported'];
    frontchannelLogoutSupported = json['frontchannel_logout_supported'];
    endSessionEndpoint = json['end_session_endpoint'];
    claimsSupported = json['claims_supported'].cast<String>();
    kerberosEndpoint = json['kerberos_endpoint'];
    tenantRegionScope = json['tenant_region_scope'];
    cloudInstanceName = json['cloud_instance_name'];
    cloudGraphHostName = json['cloud_graph_host_name'];
    msgraphHost = json['msgraph_host'];
    rbacUrl = json['rbac_url'];
  }
}
