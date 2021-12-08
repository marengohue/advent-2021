use std::io::{stdin, BufRead};
use std::collections::HashSet;

#[derive(Debug)]
struct SevenSegment {
    segments: HashSet<char>
}

impl SevenSegment {
    pub fn from(str_value: String) -> Self {
        SevenSegment {
            segments: HashSet::from_iter(str_value.chars())
        }
    }
    
    pub fn lit_count(&self) -> usize {
        self.segments.len()
    }

    pub fn is_lit(&self, segment: char) -> bool {
        self.segments.contains(&segment)
    }
}

struct Entry {
    samples: Vec<SevenSegment>,
    output: Vec<SevenSegment>
}

impl Entry {
    fn from(str_value: String) -> Self {
        let mut data_vec = str_value
            .split('|')
            .map(|seven_seg_seq| {
                seven_seg_seq
                    .trim()
                    .split(' ')
                    .map(|s| SevenSegment::from(String::from(s)))
                    .collect::<Vec<_>>()
            });
        
        Entry {
            samples: data_vec.next().unwrap(),
            output: data_vec.next().unwrap()
        }        
    }

    fn find_by_segment_count(&self, count: usize) -> &SevenSegment {
        self.samples.iter().find(|&ss| ss.segments.len() == count).unwrap()
    }
}

// Reference display
//  aaaa
// b    c
// b    c
//  dddd
// e    f
// e    f
//  gggg
fn find_remaps(entry: &Entry) -> Vec<&SevenSegment> {
    let one = entry.find_by_segment_count(2);
    let four = entry.find_by_segment_count(4);
    let seven = entry.find_by_segment_count(3);
    let eight = entry.find_by_segment_count(7);

    let nine = entry.samples.iter()
        .find(|&ss| ss.lit_count() == 6 && four.segments.difference(&ss.segments).count() == 0)
        .unwrap();

    let actual_e = *eight.segments.difference(&nine.segments).next().unwrap();
    let two = entry.samples.iter()
        .find(|&ss| ss.lit_count() == 5 && ss.is_lit(actual_e))
        .unwrap();

    let five = entry.samples.iter()
        .find(|&ss| ss.lit_count() == 5 && ss.segments.difference(&two.segments).count() == 2)
        .unwrap();

    let three = entry.samples.iter()
        .find(|&ss| ss.lit_count() == 5 && ss.segments.difference(&two.segments).count() == 1)
        .unwrap();

    let zero = entry.samples.iter()
        .find(|&ss| ss.lit_count() == 6
              && one.segments.difference(&ss.segments).count() == 0
              && ss.is_lit(actual_e)
        )
        .unwrap();

    let six = entry.samples.iter()
        .find(|&ss| ss.lit_count() == 6 && one.segments.difference(&ss.segments).count() == 1)
        .unwrap();
    
    vec![
        zero,
        one,
        two,
        three,
        four,
        five,
        six,
        seven,
        eight,
        nine
    ]
}

fn main() {
    let stdin = stdin();
    let handle = stdin.lock();

    let sum: u32 = handle.lines()
        .into_iter()
        .map(|maybe_line| {
            let line = maybe_line.unwrap();
            let entry = Entry::from(line);
            let remaps = find_remaps(&entry);
            // println!("{:?}", remaps);
            entry.output.iter()
                .fold(0, |acc, entry_out| {
                    let actual_digit = remaps.iter()
                        .enumerate()
                        .find(|&(_, &remap)| {
                            remap.segments == entry_out.segments
                        })
                        .map(|(idx, _)| idx as u32)
                        .unwrap();

                    acc * 10 + actual_digit
                })
        })
        .sum();

    println!("{}", sum);
}
