import 'package:alv_time_app/core/authentication.dart';
import 'package:flutter_test/flutter_test.dart';

void main() async {
  group("OAuthDiscoveryClient", () {
    test("Should be able to retreive discovery client", () async {
      final discoveryClient = OAuthDiscoveryClient(
          authority:
              "https://login.microsoftonline.com/76749190-4427-4b08-a3e4-161767dd1b73/v2.0");
      final discoveryDocument = await discoveryClient.getDiscoveryDocument();
      expect(discoveryDocument, (value) => value != null);

      expect(discoveryDocument!.authorizationEndpoint,
          "https://login.microsoftonline.com/76749190-4427-4b08-a3e4-161767dd1b73/oauth2/v2.0/authorize");
    });
  });
}
