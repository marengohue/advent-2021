mod input;

const LIT_PACK_ID: u8 = 4;
const SUM_PACK_ID: u8 = 0;
const PROD_PACK_ID: u8 = 1;
const MIN_PACK_ID: u8 = 2;
const MAX_PACK_ID: u8 = 3;
const GT_PACK_ID: u8 = 5;
const LT_PACK_ID: u8 = 6;
const EQ_PACK_ID: u8 = 7;

const SUBPACK_BIT_LENGTH: bool = false;
const SUBPACK_PACK_COUNT: bool = true;

struct BitStream {
    bytes: Option<Vec<u8>>,
    bits: Option<Vec<bool>>,
    offset: usize,
    max_offset: usize
}

impl Iterator for BitStream {
    type Item = bool;

    fn next(&mut self) -> Option<Self::Item> {
        if self.offset < self.max_offset {
            return match (&self.bits, &self.bytes) {
                (Some(bits), None) => {
                    let element_pos = self.offset;
                    self.offset += 1;
                    Some(bits[element_pos])
                },
                (None, Some(bytes)) => {
                    let bit_pos = self.offset % 8;
                    let element_pos = self.offset / 8;

                    self.offset += 1;
                    Some(bytes[element_pos] & (0b10000000 >> bit_pos) > 0)
                },
                _ => None
            };
        }

        None
    }
}

impl BitStream {
    fn from_bytes(bytes: Vec<u8>) -> Self {
        let max_offset = bytes.len() * 8;
        BitStream {
            bytes: Some(bytes),
            bits: None,
            offset: 0,
            max_offset
        }
    }

    fn from_bits(bits: Vec<bool>) -> Self {
        let max_offset = bits.len();
        BitStream {
            bits: Some(bits),
            bytes: None,
            offset: 0,
            max_offset
        }
    }
}

#[derive(Debug)]
enum Packet {
    Lit(Ver, u64),
    Op(Ver, Box<OpType>),
}

#[derive(Debug)]
struct Ver(u8);

#[derive(Debug)]
enum OpType {
    Sum(Vec<Packet>),
    Prod(Vec<Packet>),
    Min(Vec<Packet>),
    Max(Vec<Packet>),
    Gt(Packet, Packet),
    Lt(Packet, Packet),
    Eq(Packet, Packet),
}

fn bits_to_num(bits: Vec<bool>) -> u64 {
    bits.iter()
        .rev()
        .enumerate()
        .fold(0, |acc, (idx, &val)| {
            acc + if val { 1 << idx } else { 0 }
        })
}

fn take_x(x: usize, bit_stream: &mut impl Iterator<Item = bool>) -> Option<u64> {
    let bools = bit_stream
        .by_ref()
        .take(x)
        .collect::<Vec<_>>();
    if bools.len() == x {
        Some(bits_to_num(bools))
    } else {
        None
    }
}

fn take_3(bit_stream: &mut impl Iterator<Item = bool>) -> Option<u8> {
    take_x(3, bit_stream.by_ref()).map(|it| it as u8)
}

fn take_5(bit_stream: &mut impl Iterator<Item = bool>) -> Option<u8> {
    take_x(5, bit_stream.by_ref()).map(|it| it as u8)
}

fn take_15(bit_stream: &mut impl Iterator<Item = bool>) -> Option<u16> {
    take_x(15, bit_stream.by_ref()).map(|it| it as u16)
}

fn take_11(bit_stream: &mut impl Iterator<Item = bool>) -> Option<u16> {
    take_x(11, bit_stream.by_ref()).map(|it| it as u16)
}

fn take_literal(ver: Ver, bit_stream: &mut impl Iterator<Item = bool>) -> Option<Packet> {
    let mut buffer: u64 = 0;
    loop {
        let part = take_5(bit_stream.by_ref());
        if let Some(part_data) = part {
            buffer = buffer * 16 + (part_data & 0b0000_1111) as u64;
            if part_data & 0b0001_0000 == 0 {
                return Some(Packet::Lit(ver, buffer));
            }
        } else {
            return None;
        }           
    }
}

fn take_by_pack_count(bit_stream: &mut BitStream) -> Option<Vec<Packet>> {
    let pack_count = take_11(bit_stream.by_ref());
    if let Some(pack_count) = pack_count {
        let sub_packets = (0..pack_count)
            .filter_map(|_| take_packet(bit_stream.by_ref()))
            .collect::<Vec<_>>();
        if sub_packets.len() == pack_count.into() {
            Some(sub_packets)
        } else {
            None
        }
    } else {
        None
    }
}

fn take_by_bit_count(bit_stream: &mut BitStream) -> Option<Vec<Packet>> {
    let bit_count = take_15(bit_stream);
    if let Some(bit_count) = bit_count {
        let mut sub_stream = BitStream::from_bits(bit_stream.take(bit_count as usize).collect::<Vec<_>>());
        let sub_packets = (0..)
            .map(|_| {
                take_packet(sub_stream.by_ref())
            })
            .take_while(|it| it.is_some())
            .map(|it| it.unwrap())
            .collect::<Vec<_>>();
        if sub_packets.is_empty() {
            Some(sub_packets)
        } else {
            None
        }
    } else {
        None
    }
}

fn make_op_type(op_pack_id: u8, mut sub_packets: Vec<Packet>) -> Option<OpType> {
    match op_pack_id {
        SUM_PACK_ID => Some(OpType::Sum(sub_packets)),
        PROD_PACK_ID => Some(OpType::Prod(sub_packets)),
        MIN_PACK_ID => Some(OpType::Min(sub_packets)),
        MAX_PACK_ID => Some(OpType::Max(sub_packets)),
        _ => {
            if sub_packets.len() == 2 {
                let left = sub_packets.remove(0);
                let right = sub_packets.remove(0);
                
                match op_pack_id {
                    GT_PACK_ID => Some(OpType::Gt(left, right)),
                    LT_PACK_ID => Some(OpType::Lt(left, right)),
                    EQ_PACK_ID => Some(OpType::Eq(left, right)),
                    _ => None
                }
            } else {
                None
            }
        }
    }
}

fn take_op(op_pack_id: u8, ver: Ver, bit_stream: &mut BitStream) -> Option<Packet> {
    match bit_stream.next() {
        Some(length_type_id) => {
            let sub_packets = match length_type_id {
                 SUBPACK_BIT_LENGTH => take_by_bit_count(bit_stream.by_ref()),
                 SUBPACK_PACK_COUNT => take_by_pack_count(bit_stream.by_ref()),
            };
            if let Some(sub_packets) = sub_packets {
                make_op_type(op_pack_id, sub_packets).map(|op| {
                    Packet::Op(ver, Box::new(op))
                })
            } else {
                None
            }
        },
        _ => None
    }
}

fn take_packet(bit_stream: &mut BitStream) -> Option<Packet> {
    let version = take_3(bit_stream.by_ref());
    let pack_type = take_3(bit_stream.by_ref());

    if let Some(ver) = version {
        match pack_type {
            Some(LIT_PACK_ID) => take_literal(Ver(ver), bit_stream.by_ref()),
            Some(op_pack_id) => take_op(op_pack_id, Ver(ver), bit_stream.by_ref()),
            _ => None
        }
    } else {
        None
    }
}

fn op_version_sum(op_type: &OpType) -> u32 {
    match op_type {
        OpType::Sum(packs) => packs.iter().map(version_sum).sum(),
        OpType::Prod(packs) => packs.iter().map(version_sum).sum(),
        OpType::Min(packs) => packs.iter().map(version_sum).sum(),
        OpType::Max(packs) => packs.iter().map(version_sum).sum(),
        OpType::Gt(left, right) => version_sum(left) + version_sum(right),
        OpType::Lt(left, right) => version_sum(left) + version_sum(right),
        OpType::Eq(left, right) => version_sum(left) + version_sum(right),
    }
}

fn version_sum(packet: &Packet) -> u32 {
    match *packet {
        Packet::Lit(Ver(v), _) => v.into(),
        Packet::Op(Ver(v), ref op_type) => v as u32 + op_version_sum(op_type)
    }
}

fn eval_op(op_type: &OpType) -> u64 {
    match op_type {
        OpType::Sum(packs) => packs.iter().map(eval).sum(),
        OpType::Prod(packs) => packs.iter().map(eval).product(),
        OpType::Min(packs) => packs.iter().map(eval).min().unwrap(),
        OpType::Max(packs) => packs.iter().map(eval).max().unwrap(),
        OpType::Gt(left, right) => if eval(left) > eval(right) { 1 } else { 0 },
        OpType::Lt(left, right) => if eval(left) < eval(right) { 1 } else { 0 },
        OpType::Eq(left, right) => if eval(left) == eval(right) { 1 } else { 0 },
    }
}

fn eval(packet: &Packet) -> u64 {
    match *packet {
        Packet::Lit(_, val) => val,
        Packet::Op(_, ref op_type) => eval_op(op_type)
    }
}

fn main() {
    let input_bytes = input::read_input();

    let mut bit_stream = BitStream::from_bytes(input_bytes);
    let pack = take_packet(&mut bit_stream).unwrap();

    println!("{}", version_sum(&pack));
    println!("{}", eval(&pack));
}
