import 'package:flutter/material.dart';

import 'gradient_bloc.dart';

class TimebasedGradientBackground extends StatelessWidget {
  final Widget? child;
  final Widget? body;

  TimebasedGradientBackground({super.key, this.child, this.body});
  final GradientBloc _gradientBloc = GradientBloc();

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
        stream: _gradientBloc.timeOfDayStream,
        builder: (context, snapshot) {
          if (snapshot.hasData) {
            return Column(
              children: [
                Container(
                  width: double.infinity,
                  decoration: BoxDecoration(
                      borderRadius: const BorderRadius.only(
                          bottomLeft: Radius.circular(20),
                          bottomRight: Radius.circular(20)),
                      gradient: LinearGradient(
                        colors: snapshot.data ?? List<Color>.empty(),
                        stops: const [0, 0.5, 1],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      )),
                  child: Padding(
                    padding: const EdgeInsets.only(
                        top: 60, left: 8, right: 8, bottom: 8),
                    child: child,
                  ),
                ),
                body ??
                    const Placeholder(
                      child: Text('Some sort of drawer here'),
                    )
              ],
            );
          } else {
            return Container(child: child);
          }
        });
  }
}
