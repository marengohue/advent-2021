use std::{borrow::Borrow, collections::{HashMap, HashSet}};

use itertools::Itertools;

mod input;

pub type Path = Vec<Cave>;

#[derive(Hash, Clone, Copy, PartialEq, Eq, Debug)]
pub enum Cave {
    Start,
    End,
    Small(u64),
    Large(u64),
}

#[derive(Debug)]
pub struct CaveSystem {
    caves: HashMap<Cave, HashSet<Cave>>
}

fn is_traversible(cave: &Cave, visited_small: &HashSet<Cave>) -> bool {
    match cave {
        // Can't go again through start
        Cave::Start => false,
        
        // Only small caves that were not visited yet
        Cave::Small(_) => !visited_small
            .iter()
            .any(|it| it.eq(cave)),
        
        // Big caves and finish are allowed
        _ => true
    }
}

fn prepend_current(start_at: Cave, all_subpaths: &mut Vec<Path>) {
    for path in all_subpaths.iter_mut() {
        path.insert(0, start_at);
    }
}

fn enumerate_paths(cave_system: &CaveSystem, start_at: Cave, visited_small: HashSet<Cave>, can_double_enter: Option<Cave>) -> Vec<Path> {
    let connections= cave_system.caves[&start_at].borrow();

    connections
        .iter()
        .filter(|&it| is_traversible(it, &visited_small))
        .flat_map(|&conn| {
            match conn {
                Cave::End => vec![vec![start_at, conn]],
                Cave::Small(_) => {
                    let (visited, next_double_entry) = match &can_double_enter {
                        Some(cde) if cde.eq(&conn) => {
                            (visited_small.clone(), None)
                        },
                        _ => {
                            let mut visited_thus_far = visited_small.clone();
                            visited_thus_far.insert(conn);
                            (visited_thus_far, can_double_enter)
                        }
                    };
                    
                    let mut all_subpaths = enumerate_paths(cave_system, conn, visited, next_double_entry);
                    prepend_current(start_at, &mut all_subpaths);
                    all_subpaths
                },
                _ => {
                    let mut all_subpaths = enumerate_paths(cave_system, conn, visited_small.clone(), can_double_enter);
                    prepend_current(start_at, &mut &mut all_subpaths);
                    all_subpaths
                }
            }
        })
        .collect_vec()
}

fn paths(cave_system: &CaveSystem) -> Vec<Path> {
    cave_system.caves.iter()
        .filter_map(|(cave, _)| {
            match cave {
                Cave::Small(_) => Some(cave),
                _ => None
            }
        })
        .flat_map(|&small_double_enter| {
            enumerate_paths(cave_system, Cave::Start, HashSet::<Cave>::new(), Some(small_double_enter))
        })
        .unique()
        .collect::<Vec<_>>()
}

fn main() {
    let cave_system = input::read_input();
    println!("{}", paths(&cave_system).len());
}
