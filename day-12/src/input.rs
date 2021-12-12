use std::{
    collections::{
        HashMap, HashSet, hash_map::DefaultHasher
    },
    hash::{Hash, Hasher},
    io::{BufRead, stdin}
};

use crate::{Cave, CaveSystem};

fn read_cave(cave_str: &str) -> Cave {
    if cave_str == "start" {
        Cave::Start
    } else if cave_str == "end" {
        Cave::End
    } else {
        // Using hasher to make the cave enum be easily Copyable
        let mut hasher = DefaultHasher::new();
        cave_str.hash(&mut hasher);
        let cave_id = hasher.finish();
        
        if cave_str.chars().all(char::is_uppercase) {
            Cave::Large(cave_id)
        } else {
            Cave::Small(cave_id)
        }
    }
}

fn link_caves(caves: &mut HashMap<Cave, HashSet<Cave>>, c1: Cave, c2: Cave) {
    caves
        .entry(c1)
        .and_modify(|existing| {
            existing.insert(c2);
        })
        .or_insert_with(|| {
            let mut set = HashSet::new();
            set.insert(c2);
            set
        });
}

pub fn read_input() -> CaveSystem {
    let stdin = stdin();
    let handle = stdin.lock();

    let mut caves = HashMap::<Cave, HashSet<Cave>>::new();
    
    handle
        .lines()
        .into_iter()
        .filter_map(|it| {
            match it {
                Ok(line) => Some(line),
                _ => None
            }
        })
        .for_each(|path_line| {
            let mut parts = path_line.split('-');
            let from = read_cave(parts.next().unwrap());
            let to = read_cave(parts.next().unwrap());
            link_caves(&mut caves, from, to);
            link_caves(&mut caves, to, from);
        });

    CaveSystem { caves }    
}
