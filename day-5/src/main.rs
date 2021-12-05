mod input;
use std::collections::HashMap;

pub type Point = (i32, i32);

#[derive(Debug)]
pub struct Line {
    from: Point,
    to: Point
}

impl Line {
    pub fn new(from: Point, to: Point) -> Self {
        Line { from, to }
    }

    pub fn is_straight(&self) -> bool {
        self.from.0 == self.to.0 || self.from.1 == self.to.1
    }
    
    pub fn rasterize(&self) -> impl Iterator<Item = Point> {
        let (x_from, y_from) = self.from;
        let (x_to, y_to) = self.to;

        let point_count = (x_to - x_from).abs().max((y_to - y_from).abs());
        let x_step = (x_to - x_from) as f32 / point_count as f32;
        let y_step = (y_to - y_from) as f32 / point_count as f32;

        (0..=point_count)
            .map(move |idx| (
                ((x_from as f32) + 0.5 + x_step * idx as f32).floor() as i32,
                ((y_from as f32) + 0.5 + y_step * idx as f32).floor() as i32
            ))
    }
}
    
fn main() {
    let lines = input::read_inputs();
    let mut counts = HashMap::<Point, usize>::new();

    lines.iter()
        // Could filter away straight lines,
        // But the part1/part2 split this time around
        // seems a bit too artificial
        .flat_map(|v| v.rasterize())
        .for_each(|p| {
            *counts.entry(p).or_insert(0) += 1;
        });

    let overlaps = counts.iter()
        .filter(|(_, &count)| count > 1)
        .count();

    
    println!("{:?}", overlaps);
}
    
