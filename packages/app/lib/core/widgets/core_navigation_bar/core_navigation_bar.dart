import 'package:alv_time_app/routes.dart';
import 'package:flutter/material.dart';

class CoreNavigationBar extends StatelessWidget {
  const CoreNavigationBar({super.key});

  @override
  Widget build(BuildContext context) {
    String? currentRoute = ModalRoute.of(context)?.settings.name;

    return NavigationBar(
        selectedIndex: () {
          switch (currentRoute) {
            case routeHomeScreen:
              return 0;
            case routeProfileScreen:
              return 2;
            default:
              return 0;
          }
          ;
        }(),
        backgroundColor: Colors.transparent,
        elevation: 30,
        onDestinationSelected: (index) {
          switch (index) {
            case 0:
              Navigator.pushReplacementNamed(context, "/");
              break;
            case 2:
              Navigator.pushReplacementNamed(context, "/profile");
              break;
          }
        },
        destinations: const [
          NavigationDestination(
              icon: Icon(Icons.home_filled), label: 'Oversikt'),
          NavigationDestination(
              icon: Icon(Icons.bar_chart), label: 'Statistikk'),
          NavigationDestination(icon: Icon(Icons.person), label: 'Profil'),
        ]);
  }
}
