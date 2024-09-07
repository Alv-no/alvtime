import 'package:flutter/material.dart';
import 'package:carousel_slider/carousel_slider.dart';

class TaskGroup {
  final String title;
  final String color;
  final int index;

  TaskGroup({required this.title, required this.color, required this.index});
}

class Carousel extends StatelessWidget {
  final CarouselSliderController _controller = CarouselSliderController();

  @override
  Widget build(BuildContext context) {
    return CarouselSlider(
      carouselController: _controller,
      options: CarouselOptions(
        height: 150.0,
        autoPlay: false,
        enlargeCenterPage: false,
        autoPlayInterval: const Duration(seconds: 3),
        autoPlayAnimationDuration: const Duration(milliseconds: 800),
        autoPlayCurve: Curves.fastOutSlowIn,
        scrollDirection: Axis.horizontal,
        initialPage: 1,
        enableInfiniteScroll: false,
        viewportFraction: .6,
      ),
      items: <TaskGroup>[TaskGroup(title: "Fravær", color: "0xffbd00ff", index: 0), TaskGroup(title: "Kunder", color: "0xff23b100", index: 1), TaskGroup(title: "Internt", color: "0xff00c2ff", index: 2)].map((item) {
        return Builder(
          builder: (BuildContext context) {
            return Container(
              width: MediaQuery.of(context).size.width,
              margin: const EdgeInsets.symmetric(horizontal: 20.0),
              decoration: BoxDecoration(
                color: Color(int.parse("0xffd9d9d9")),
                borderRadius: BorderRadius.circular(10.0),
              ),
              child: Row(
                children: [
                  if (item.index != 2) ...[
                  Center(
                    child: Container(
                      margin: const EdgeInsets.only(left: 10.0),
                      child: Text(
                        item.title,
                        style: const TextStyle(fontSize: 16.0),
                      ),
                    ),
                  ),
                  const Spacer(),
                  Container(
                    padding: const EdgeInsets.only(right: 10, left: 10),
                    height: double.infinity,
                    decoration: BoxDecoration(
                      color: Color(int.parse(item.color)),
                      borderRadius: BorderRadius.circular(10.0),
                    ),
                    margin: const EdgeInsets.only(left: 10.0),
                    child: RotatedBox(quarterTurns: -1 ,child: Center(child: Text(item.title))),
                  ),
                ] else ...[
                  Container(
                    padding: const EdgeInsets.only(right: 10, left: 10),
                    height: double.infinity,
                    decoration: BoxDecoration(
                      color: Color(int.parse(item.color)),
                      borderRadius: BorderRadius.circular(10.0),
                    ),
                    child: RotatedBox(quarterTurns: -1 ,child: Center(child: Text(item.title))),
                  ),
                  const Spacer(),
                  Center(
                    child: Container(
                      margin: const EdgeInsets.only(right: 10.0),
                      child: Text(
                        item.title,
                        style: const TextStyle(fontSize: 16.0),
                      ),
                    ),
                  ),
                ]],
              ),
            );
          },
        );
      }).toList(),
    );
  }
}
