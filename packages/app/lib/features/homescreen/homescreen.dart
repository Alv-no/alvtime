import 'package:flutter/material.dart';
import 'package:alv_time_app/features/homescreen/carousel.dart';
import 'package:alv_time_app/features/homescreen/widgets/gradient/gradient.dart';
import 'package:alv_time_app/const.dart';

class HomeScreenWidget extends StatelessWidget {
  const HomeScreenWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      extendBodyBehindAppBar: true,
      appBar: AppBar(
        title: const Text(
          appName,
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
        ),
        backgroundColor: Colors.transparent,
        elevation: 0,
      ),
      body: TimebasedGradientBackground(
        body: Carousel(),
        child: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Text(
                'Faktureringsgrad',
                style: TextStyle(color: Colors.white, fontSize: 40),
              ),
              const SizedBox(height: 20),
            ],
          ),
        ),
      ),
      bottomNavigationBar: NavigationBar(
        backgroundColor: Colors.transparent,
        elevation: 30,
        destinations: const [
          NavigationDestination(
              icon: Icon(Icons.home_filled), label: 'Oversikt'),
          NavigationDestination(
              icon: Icon(Icons.bar_chart), label: 'Statistikk'),
          NavigationDestination(icon: Icon(Icons.person), label: 'Profil'),
        ],
      ),
    );
  }
}
