import 'package:alv_time_app/const.dart';
import 'package:alv_time_app/features/homescreen/widgets/gradient/gradient.dart';
import 'package:flutter/material.dart';

class ProfileScreenWidget extends StatelessWidget {
  const ProfileScreenWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: TimebasedGradientBackground(
          child: const Text("Profil",
              style: TextStyle(color: Colors.white, fontSize: 40))),
      appBar: AppBar(
          title: const Text(
        appName,
        style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
      )),
      bottomNavigationBar: NavigationBar(
          backgroundColor: Colors.transparent,
          elevation: 30,
          destinations: const [
            NavigationDestination(
                icon: Icon(Icons.home_filled), label: 'Oversikt'),
            NavigationDestination(
                icon: Icon(Icons.bar_chart), label: 'Statistikk'),
            NavigationDestination(icon: Icon(Icons.person), label: 'Profil'),
          ]),
    );
  }
}
