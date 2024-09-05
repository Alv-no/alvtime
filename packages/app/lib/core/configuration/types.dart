class AuthenticationConfiguration {
  final String authority;
  final String clientId;
  final String redirectUrl;
  final String mobileCustomScheme;
  final String mobileRedirectUri;
  final String customRedirectScheme;
  final List<String> scopes;

  AuthenticationConfiguration(
      {required this.authority,
      required this.clientId,
      required this.redirectUrl,
      required this.scopes,
      required this.customRedirectScheme,
      required this.mobileRedirectUri,
      required this.mobileCustomScheme});
}

class AppConfiguration {
  final AuthenticationConfiguration authenticationConfiguration;
  final Uri backendUri;

  AppConfiguration(
      {required this.authenticationConfiguration, required this.backendUri});
}
