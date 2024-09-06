import 'dart:ui';
import 'package:rxdart/rxdart.dart';

const _morningGradient = [
  Color(0xff5433ff),
  Color(0xff20bdff),
  Color(0xff00f5ff)
];

const _dayGradient = [Color(0xff0052d4), Color(0xff4364f7), Color(0xff6fb1fc)];

const _eveningGradient = [
  Color(0xff000c40),
  Color(0xff607d8b),
  Color(0xff607d8b)
];

const _nightGradient = [
  Color(0xff0f2027),
  Color(0xff203a43),
  Color(0xff2c5364)
];

class CoreGradientBloc {
  final Stream<List<Color>> timeOfDayStream =
      Stream.periodic(const Duration(hours: 1)).startWith(1).map((_) {
    final hour = DateTime.now().hour;

    if (hour > 6 && hour < 12) {
      return _morningGradient;
    } else if (hour >= 12 && hour < 18) {
      return _dayGradient;
    } else if (hour >= 18 && hour < 22) {
      return _eveningGradient;
    } else {
      return _nightGradient;
    }
  });
}
