#![feature(drain_filter)]

use std::io::{BufRead, stdin};

const BIT_SIZE: usize = 32;

fn epsilon_from_gamma(gamma: u32, len: u32) -> u32 {
    // number with 'len' ones is 2**len - 1 
    let inverter = 2_u32.pow(len) - 1;
    // XOR with all ones -> invert bits
    gamma ^ inverter
}

fn bit_at(num: u32, pos: usize) -> u32 {
    let mask = 1_u32 << pos;
    if num & mask > 0 { 0b1 } else { 0b0 }
}

fn apply_bits(freqs: &mut Vec<u32>, num: u32) {
    for bit_no in 0..BIT_SIZE {
        if bit_at(num, bit_no) == 0b1 {
            freqs[bit_no] += 1;
        }
    }
}

#[derive(Clone, Copy)]
enum AnalysisKind {
    MostFrequent,
    LeastFrequent
}

fn analyze_freq(inputs: &[u32], max_size: usize, kind: AnalysisKind) -> Vec<u32> {
    let mut counts = vec![0; BIT_SIZE];
    inputs
        .iter()
        .for_each(|&next_number| apply_bits(&mut counts, next_number));

    counts
        .iter()
        .map(|&bit_freq| {
            let threshold_condition = match kind {
                AnalysisKind::MostFrequent => 2 * bit_freq >= inputs.len() as u32,
                AnalysisKind::LeastFrequent => 2 * bit_freq < inputs.len() as u32
            };
            if threshold_condition { 1 } else { 0 }
        })
        .take(max_size)
        .collect::<Vec<_>>()
}

fn read_inputs() -> (Vec<u32>, usize) {
    let stdin = stdin();
    let handle = stdin.lock();

    let mut max_len = 0;
    let diagnostics = handle.lines()
        .map(|maybe_line| {
            let line = maybe_line.unwrap();
            max_len = max_len.max(line.len());
            isize::from_str_radix(&line, 2).unwrap() as u32
        })
        .collect::<Vec<_>>();
    
    (diagnostics, max_len)
}

fn extract_life_supp_numbers(mut diagnostics: Vec<u32>, max_len: usize, kind: AnalysisKind) {
    for bit_no in (0..max_len).rev() {
        let freqs = analyze_freq(&diagnostics, max_len, kind);
        diagnostics.drain_filter(|&mut number| {
            bit_at(number, bit_no) != freqs[bit_no]
        });
        if diagnostics.len() == 1 {
            println!("{:?}", diagnostics);
            break;
        }
    }
}

fn main() {
    let (mut diagnostics, max_len) = read_inputs();
    let global_freqs = analyze_freq(&diagnostics, max_len, AnalysisKind::MostFrequent);

    // Subdivide on the first bit, obtaining two sets responsible
    // for two parts of the answer
    let diagnostics_remainder = diagnostics.drain_filter(|&mut number| {
        bit_at(number, max_len - 1) != global_freqs[max_len - 1]
    }).collect::<Vec<_>>();

    extract_life_supp_numbers(diagnostics, max_len - 1, AnalysisKind::MostFrequent);
    extract_life_supp_numbers(diagnostics_remainder, max_len - 1, AnalysisKind::LeastFrequent);
    
    let gamma = global_freqs
        .iter()
        .enumerate()
        .fold(0, |acc, (idx, bit)| acc + 2_u32.pow(idx as u32) * bit);

    println!("{}, {}", gamma, epsilon_from_gamma(gamma, max_len as u32));
}
    
